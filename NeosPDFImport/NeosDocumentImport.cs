using BaseX;
using CodeX;
using FrooxEngine;
using HarmonyLib;
using NeosModLoader;
using NeosDocumentImport;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NeosDocumentImport
{
    public class NeosDocumentImport : NeosMod
    {
        public override string Name => "NeosDocumentImport";
        public override string Author => "mpmxyz";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/mpmxyz/RepoName/"; //TODO
        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("com.github.mpmxyz.documentimport");
            harmony.PatchAll();
            Converters.Register(new PDFConverter(), AssetClass.Document, ".pdf");
            Converters.Register(new SVGConverter(), AssetClass.Unknown, ".svg", "svgz");
            var htmlConverter = new HTMLConverter();
            Converters.Register(htmlConverter, AssetClass.Text, ".html", ".xhtml", ".htm", ".xht");
            Converters.Register(htmlConverter, AssetClass.Unknown, "");
        }


        [HarmonyPatch(typeof(UniversalImporter), "ImportTask")]
        class NeosPDFImportPatch
        {
            public static bool Prefix(ref Task __result, AssetClass assetClass, ref IEnumerable<string> files, World world, float3 position, floatQ rotation, float3 scale, bool silent = false)
            {
                var appliedConverters = files.GroupBy((file) => Converters.Get(assetClass, file));
                files = new List<string>();
                
                foreach (var converterFiles in appliedConverters)
                {
                    var converter = converterFiles.Key;
                    if (converter != null)
                    {
                        ImportConfigurator.Spawn(converterFiles, world, position, rotation, scale, DocumentImporter.Spawn, converter);
                    }
                    else
                    {
                        files = converterFiles.ToList();
                    }
                }

                if (files.Any())
                {
                    return true;
                }
                else
                {
                    __result = new Task(() => { });
                    return false;
                }
            }
        }
    }
}