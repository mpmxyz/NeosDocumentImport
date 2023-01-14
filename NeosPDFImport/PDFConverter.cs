
using BaseX;
using Docnet.Core.Models;
using Docnet.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Docnet.Core.Converters;
using Docnet.Core.Readers;
using System.Drawing;
using System.Runtime.InteropServices;
using NeosDocumentImport;

namespace NeosDocumentImport
{
    class PDFConverter : IConverter
    {
        private const string PAGE_SUFFIX = ".png";

        public List<string> Apply(string file, string outputDir, string pagePrefix, ImportConfig config, IProgressIndicator progress)
        {
            using (var doc = DocLib.Instance.GetDocReader(file, new PageDimensions(config.ppi / 72.0)))
            {
                Directory.CreateDirectory(outputDir);

                var nPages = doc.GetPageCount();
                var pageNumberLength = nPages.ToString().Length;
                var filename = Path.GetFileName(file);

                var pages = config.pages;
                if (!pages.Any())
                {
                    pages = new List<PageRange> { new PageRange(1, nPages) };
                }
                pages = pages.ConvertAll((range) => range.Clamp(1, nPages));
                pages.RemoveAll((range) => range == null);

                var nRequested = pages.Sum((x) => x.Count);
                DocumentImporter.UpdateProgress(progress, filename, 0, nRequested);

                var outputFiles = new List<string>();

                foreach (var range in pages)
                {
                    foreach (var iPage in range)
                    {
                        using (var page = doc.GetPageReader(iPage - 1))
                        {
                            var pageNumber = iPage.ToString();
                            pageNumber = new string('0', pageNumberLength - pageNumber.Length) + pageNumber;
                            var outputFile = Path.Combine(outputDir, $"{pagePrefix}_{pageNumber}{PAGE_SUFFIX}");

                            if (!outputFiles.Contains(outputFile))
                            {
                                using (var image = Rasterize(page, config)) {
                                    image.Save(outputFile);
                                }
                            }

                            outputFiles.Add(outputFile);

                            DocumentImporter.UpdateProgress(progress, filename, outputFiles.Count(), nRequested);
                        }
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

        private static Image Rasterize(IPageReader page, ImportConfig config)
        {
            var bitmap = new Bitmap(
                page.GetPageWidth(),
                page.GetPageHeight(),
                System.Drawing.Imaging.PixelFormat.Format32bppArgb
            );

            var rendered = config.opaque ? page.GetImage(new OpaqueConverter()) : page.GetImage();
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
