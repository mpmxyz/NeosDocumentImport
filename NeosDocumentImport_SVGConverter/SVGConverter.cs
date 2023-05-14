using BaseX;
using FrooxEngine;
using NeosDocumentImport;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NeosDocumentImport_SVGConverter
{
    /// <summary>
    /// configures and applies conversion of a svg files to images
    /// </summary>
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

        public async Task<List<string>> Apply(string file, string outputDir, string pagePrefix, IProgressIndicator progress)
        {
            var filename = Path.GetFileName(file);
            var outputFile = Path.Combine(outputDir, $"{pagePrefix}.png");
            ImportUtils.Update(progress, filename, 0, "Loading File...");

            var data = await ImportUtils.LoadData(file, world);
            ImportUtils.Update(progress, filename, 0, "Loading Data...");

            using (var helper = new SVGHelper(data))
            {
                ImportUtils.Update(progress, filename, 1f / 3, "Rendering...");

                helper.Render(width);

                if (!transparency)
                {
                    ImportUtils.Update(progress, filename, 1.5f / 3, "Removing Transparency...");
                    helper.MakeOpaque();
                }
                ImportUtils.Update(progress, filename, 2 / 3f, "Writing file...");

                helper.Export(outputFile);
                ImportUtils.Update(progress, filename, 1, "Done!");

                return new List<string> { outputFile };
            }
        }

        public bool ValidateConfig()
        {
            return width > 0;
        }
    }
}
