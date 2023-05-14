using Svg;
using System;
using System.Drawing;
using System.IO;

namespace NeosDocumentImport_SVGConverter
{
    /// <summary>
    /// adapter to Svg library
    /// </summary>
    internal class SVGHelper : IDisposable
    {
        private SvgDocument svgDoc;
        private Bitmap image = null;

        public SVGHelper(byte[] data)
        {
            using (Stream stream = new MemoryStream(data))
            {
                svgDoc = SvgDocument.Open<SvgDocument>(stream);
            }
        }

        internal void Render(int width)
        {
            image = svgDoc.Draw(width, 0);
        }

        internal void MakeOpaque()
        {
            if (image == null)
            {
                throw new InvalidOperationException();
            }

            using (var copy = new Bitmap(image))
            {
                using (var gfx = Graphics.FromImage(image))
                {
                    gfx.FillRectangle(new SolidBrush(Color.White), gfx.ClipBounds);
                    gfx.DrawImage(copy, Point.Empty);
                };
            }
        }

        internal void Export(string outputFile)
        {
            if (image == null)
            {
                throw new InvalidOperationException();
            }

            image.Save(outputFile);
        }

        public void Dispose()
        {
            image?.Dispose();
        }

    }
}