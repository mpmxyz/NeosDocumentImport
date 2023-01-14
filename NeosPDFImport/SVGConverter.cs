using BaseX;
using Svg;
using System.Collections.Generic;
using System.IO;

namespace NeosDocumentImport
{
    internal class SVGConverter : IConverter
    {
        public List<string> Apply(string file, string outputDir, string pagePrefix, ImportConfig config, IProgressIndicator progress)
        {
            var svgDoc = SvgDocument.Open(file);
            var image = svgDoc.Draw(config.ppi, 0); //height=0 or width=0 keeps aspect ratio
            var outputFile = Path.Combine(outputDir, $"{pagePrefix}.png");

            Directory.CreateDirectory(outputDir);
            image.Save(outputFile);

            return new List<string> { outputFile };
        }
    }
}
