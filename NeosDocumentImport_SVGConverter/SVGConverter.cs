using BaseX;
using FrooxEngine;
using Svg;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace NeosDocumentImport
{
    internal class SVGConverter : IConverter
    {
        [Range(32, 512, "0")]
        [Config("Width", ConfigType.Value)]
        private int width = 128;
        [Config("Transparency", ConfigType.Value)]
        private bool transparency = true;


        private readonly World world;

        public SVGConverter(World world)
        {
            this.world = world;
        }

        public List<string> Apply(string file, string outputDir, string pagePrefix, IProgressIndicator progress)
        {
            var filename = Path.GetFileName(file);
            DocumentImporter.UpdateProgress(progress, filename, 0, "Loading File...");

            var data = ImportUtils.LoadData(file, world);

            DocumentImporter.UpdateProgress(progress, filename, 0, "Loading Data...");
            SvgDocument svgDoc;
            using (System.IO.Stream stream = new MemoryStream(data))
            {
                svgDoc = SvgDocument.Open<SvgDocument>(stream);
            }

            DocumentImporter.UpdateProgress(progress, filename, 1f/3, "Rendering...");
            using(var image = svgDoc.Draw(width, 0)) //height=0 or width=0 keeps aspect ratio
            {

                var outputFile = Path.Combine(outputDir, $"{pagePrefix}.png");
                Directory.CreateDirectory(outputDir);

                if (!transparency)
                {
                    DocumentImporter.UpdateProgress(progress, filename, 1.5f / 3, "Removing Transparency...");
                    using (var copy = new Bitmap(image))
                    {
                        using (var gfx = Graphics.FromImage(image))
                        {
                            gfx.FillRectangle(new SolidBrush(Color.White), gfx.ClipBounds);
                            gfx.DrawImage(copy, System.Drawing.Point.Empty);
                        };
                    }
                }

                DocumentImporter.UpdateProgress(progress, filename, 2 / 3f, "Writing file...");

                image.Save(outputFile);

                DocumentImporter.UpdateProgress(progress, filename, 1, "Done!");

                return new List<string> { outputFile };
            }
        }

        public bool ValidateConfig()
        {
            return width > 0;
        }
    }
}
