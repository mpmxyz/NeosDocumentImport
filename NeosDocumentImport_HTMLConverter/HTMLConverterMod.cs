using CodeX;
using NeosModLoader;

namespace NeosDocumentImport
{
    public class HTMLConverterMod : NeosMod
    {
        public override string Name => "NeosDocumentImport_PDFConverter";
        public override string Author => "mpmxyz";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/mpmxyz/NeosDocumentImport/";
        public override void OnEngineInit()
        {
            var htmlConverter = new HTMLConverter();
            Converters.Register(htmlConverter, AssetClass.Text, ".html", ".xhtml", ".htm", ".xht");
            Converters.Register(htmlConverter, AssetClass.Unknown, "");
        }
    }
}
