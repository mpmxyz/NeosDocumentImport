using BaseX;
using Docnet.Core;
using Docnet.Core.Converters;
using Docnet.Core.Models;
using Docnet.Core.Readers;
using FrooxEngine;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace NeosDocumentImport
{
    class PDFConverter : IConverter
    {
        private const string PAGE_SUFFIX = ".png";

        [Range(50, 300, "0")]
        [Config("PPI", ConfigType.Value)]
        private int ppi = 150;
        [Config("Background", ConfigType.Value)]
        private bool background = true;
        [Config("Pages", ConfigType.Value)]
        private string rawPages = null;

        private List<PageRange> pages = new List<PageRange>();

        private readonly World world;

        public PDFConverter(World world)
        {
            this.world = world;
        }

        public bool ValidateConfig()
        {
            pages = PageRange.fromString(rawPages);
            return ppi > 0 && pages != null;
        }

        public List<string> Apply(string file, string outputDir, string pagePrefix, IProgressIndicator progress)
        {
            var filename = Path.GetFileName(file);
            
            DocumentImporter.UpdateProgress(progress, filename, 0, "Loading File...");

            var data = ImportUtils.LoadData(file, world);

            DocumentImporter.UpdateProgress(progress, filename, 0, "Loading Data...");

            var dimensions = new PageDimensions(ppi / 72.0);
            using (var doc = DocLib.Instance.GetDocReader(data, dimensions))
            {
                var nPages = doc.GetPageCount();
                var pageNumberLength = nPages.ToString().Length;

                if (!pages.Any())
                {
                    pages = new List<PageRange> { new PageRange(1, nPages) };
                }

                pages = pages.ConvertAll((range) => range.Clamp(1, nPages));
                pages.RemoveAll((range) => range == null);

                var outputFiles = new List<string>();
                var nDone = 0;
                var nRequested = pages.Sum((x) => x.Count);
                float percentage() => (float)nDone / nRequested;

                foreach (var range in pages)
                {
                    foreach (var iPage in range)
                    {
                        var pageNumber = iPage.ToString();
                        pageNumber = new string('0', pageNumberLength - pageNumber.Length) + pageNumber;
                        var outputFile = Path.Combine(outputDir, $"{pagePrefix}_{pageNumber}{PAGE_SUFFIX}");

                        if (!outputFiles.Contains(outputFile))
                        {
                            DocumentImporter.UpdateProgress(progress, $"{filename} ({nDone + 1}/{nRequested})", percentage(), "Loading... ");
                            using (var page = doc.GetPageReader(iPage - 1))
                            {
                                DocumentImporter.UpdateProgress(progress, $"{filename} ({nDone + 1}/{nRequested})", percentage(), "Drawing...");
                                using (var image = Rasterize(page))
                                {
                                    DocumentImporter.UpdateProgress(progress, $"{filename} ({nDone + 1}/{nRequested})", percentage(), "Saving...");
                                    image.Save(outputFile);
                                }
                            }
                        }

                        outputFiles.Add(outputFile);
                        nDone++;

                        DocumentImporter.UpdateProgress(progress, $"{filename} ({nDone}/{nRequested})", percentage(), "Done!");
                    }
                }

                return outputFiles;
            }
        }

        private class OpaqueConverter : IImageBytesConverter
        {
            public byte[] Convert(byte[] bytes)
            {
                for (int i = 0; i < bytes.Length - 3; i += 4)
                {
                    int alpha = bytes[i + 3];
                    int antiAlpha = 255 - alpha; //white background, premultiplied
                    bytes[i] = (byte)(bytes[i] * alpha / 255 + antiAlpha);
                    bytes[i + 1] = (byte)(bytes[i + 1] * alpha / 255 + antiAlpha);
                    bytes[i + 2] = (byte)(bytes[i + 2] * alpha / 255 + antiAlpha);
                    bytes[i + 3] = 255;
                }
                return bytes;
            }
        }

        private Image Rasterize(IPageReader page)
        {
            var bitmap = new Bitmap(
                page.GetPageWidth(),
                page.GetPageHeight(),
                System.Drawing.Imaging.PixelFormat.Format32bppArgb
            );

            var rendered = background ? page.GetImage(new OpaqueConverter()) : page.GetImage();
            var bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                bitmap.PixelFormat
            );
            var bitmapBytes = bitmapData.Scan0;

            Marshal.Copy(rendered, 0, bitmapBytes, rendered.Length);
            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }
    }
}
