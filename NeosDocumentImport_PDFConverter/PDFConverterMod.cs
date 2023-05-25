using CodeX;
using NeosDocumentImport;
using NeosModLoader;

namespace NeosDocumentImport_PDFConverter
{
    public class PDFConverterMod : NeosMod
    {
        public override string Name => "NeosDocumentImport_PDFConverter";
        public override string Author => "mpmxyz";
        public override string Version => "4.0.0";
        public override string Link => "https://github.com/mpmxyz/NeosDocumentImport/";
        public override void OnEngineInit()
        {
            Converters.Register((world) => new PDFConverter(world), AssetClass.Document, "pdf");
        }
    }
}
