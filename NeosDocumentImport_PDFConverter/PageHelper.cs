using System;
using Docnet.Core.Readers;
using Docnet.Core.Converters;
using System.Drawing;
using System.Runtime.InteropServices;

namespace NeosDocumentImport_PDFConverter
{
    /// <summary>
    /// page related adapter to Docnet.Core library
    /// </summary>
    internal class PageHelper : IDisposable
    {
        private readonly IPageReader pageReader;
        private Bitmap bitmap;

        public PageHelper(IPageReader pageReader)
        {
            this.pageReader = pageReader;
        }

        public void Dispose()
        {
            pageReader.Dispose();
        }

        internal void Render(bool withWhiteBackground)
        {
            bitmap = new Bitmap(
                pageReader.GetPageWidth(),
                pageReader.GetPageHeight(),
                System.Drawing.Imaging.PixelFormat.Format32bppArgb
            );

            var rendered = withWhiteBackground ? pageReader.GetImage(OpaqueConverter.Instance) : pageReader.GetImage();
            var bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                bitmap.PixelFormat
            );
            var bitmapBytes = bitmapData.Scan0;

            if (bitmap.Width * bitmap.Height * 4 != rendered.Length)
            {
                //this could have resulted in unsafe memory copies
                throw new InvalidOperationException();
            }

            Marshal.Copy(rendered, 0, bitmapBytes, rendered.Length);
            bitmap.UnlockBits(bitmapData);
        }

        internal void Save(string outputFile)
        {
            if (bitmap == null)
            {
                throw new InvalidOperationException();
            }

            bitmap.Save(outputFile);
        }


        private class OpaqueConverter : IImageBytesConverter
        {
            internal static readonly OpaqueConverter Instance = new OpaqueConverter();

            public byte[] Convert(byte[] bytes)
            {
                for (int i = 0; i < bytes.Length - 3; i += 4)
                {
                    int alpha = bytes[i + 3];
                    int antiAlpha = 255 - alpha; //white background, premultiplied with alpha
                    bytes[i] = (byte)(bytes[i] * alpha / 255 + antiAlpha);
                    bytes[i + 1] = (byte)(bytes[i + 1] * alpha / 255 + antiAlpha);
                    bytes[i + 2] = (byte)(bytes[i + 2] * alpha / 255 + antiAlpha);
                    bytes[i + 3] = 255;
                }
                return bytes;
            }
        }
    }
}
