using System;
using System.Drawing;
using PdfLibCore;

namespace NeosDocumentImport_PDFConverter
{
    /// <summary>
    /// page related adapter to Docnet.Core library
    /// </summary>
    internal class PageHelper : IDisposable
    {
        private readonly PdfPage page;
        private Bitmap bitmap = null;

        public PageHelper(PdfPage page)
        {
            this.page = page;
        }

        public void Dispose()
        {
            using (var p = page) //can't dispose directly...
            {
                bitmap?.Dispose();
            }
        }

        internal void Render(int ppi)
        {
            bitmap?.Dispose();

            int width = (int)(page.Width * ppi / 72);
            int height = (int)(page.Height * ppi / 72);
            bitmap = new Bitmap(
                width,
                height,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb
            );
            var bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                bitmap.PixelFormat
            );
            using (var pdfiumBitmap = new PdfiumBitmap(width, height, PdfLibCore.Enums.BitmapFormats.RGBA, bitmapData.Scan0, bitmapData.Stride))
            {
                page.Render(pdfiumBitmap);
            }
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
    }
}
