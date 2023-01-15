using CodeX;
using NeosModLoader;

namespace NeosDocumentImport
{
    public class SVGConverterMod : NeosMod
    {
        public override string Name => "NeosDocumentImport_SVGConverter";
        public override string Author => "mpmxyz";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/mpmxyz/NeosDocumentImport/";
        public override void OnEngineInit()
        {
            Converters.Register(new SVGConverter(), AssetClass.Unknown, ".svg", "svgz");
        }
    }
}
