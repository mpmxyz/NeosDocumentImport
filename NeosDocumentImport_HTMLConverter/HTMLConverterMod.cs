using BaseX;
using CodeX;
using FrooxEngine;
using NeosModLoader;
using PuppeteerSharp;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NeosDocumentImport
{
    public class HTMLConverterMod : NeosMod
    {
        public override string Name => "NeosDocumentImport_HTMLConverter";
        public override string Author => "mpmxyz";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/mpmxyz/NeosDocumentImport/";

        public override void OnEngineInit()
        {
            var options = new BrowserFetcherOptions
            {
                Path = Path.Combine(Directory.GetCurrentDirectory(), "nml_config/.NeosDocumentImport/browser")
            };
            var browserFetcher = new BrowserFetcher(options);
            UniLog.Log("NeosDocumentImportMod: Fetching Browser for HTML -> Image conversion...");
            var browserDownload = browserFetcher.DownloadAsync();
            Task.Run(async () =>
            {
                await browserDownload;
                UniLog.Log("NeosDocumentImportMod: Browser Downloaded!");
            });

            HTMLConverter htmlConverter(World world) => new HTMLConverter(browserDownload);
            Converters.Register((Func<World, HTMLConverter>)htmlConverter, AssetClass.Text, ".html", ".xhtml", ".htm", ".xht");
            Converters.Register((Func<World, HTMLConverter>)htmlConverter, AssetClass.Unknown, "");


        }

    }
}
