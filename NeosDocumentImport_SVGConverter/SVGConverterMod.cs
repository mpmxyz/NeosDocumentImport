using CodeX;
using NeosDocumentImport;
using NeosModLoader;

namespace NeosDocumentImport_SVGConverter
{
    public class SVGConverterMod : NeosMod
    {
        public override string Name => "NeosDocumentImport_SVGConverter";
        public override string Author => "mpmxyz";
        public override string Version => "3.0.0";
        public override string Link => "https://github.com/mpmxyz/NeosDocumentImport/";
        public override void OnEngineInit()
        {
            Converters.Register((world) => new SVGConverter(world), AssetClass.Document, "svg", "svgz");
            AssetClassOverrides.Register(AssetClass.Document, "svg", "svgz");
        }
    }
}
