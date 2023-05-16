using BaseX;
using CodeX;
using FrooxEngine;
using HarmonyLib;
using NeosModLoader;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NeosDocumentImport
{
    public class NeosDocumentImportMod : NeosMod
    {
        public override string Name => "NeosDocumentImport";
        public override string Author => "mpmxyz";
        public override string Version => "3.0.0";
        public override string Link => "https://github.com/mpmxyz/NeosDocumentImport/";
        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("com.github.mpmxyz.documentimport");
            harmony.PatchAll();
        }

        public static bool skipNext = false;

        [HarmonyPatch(typeof(UniversalImporter), "ImportTask")]
        class ImportTaskPatch
        {
            public static bool Prefix(ref Task __result, AssetClass assetClass, ref IEnumerable<string> files, World world, float3 position, floatQ rotation, float3 scale, bool silent = false)
            {
                if (skipNext)
                {
                    skipNext = false;
                    return true;
                }

                var appliedConverters = files.GroupBy((file) => Converters.GetFactory(assetClass, file));
                files = new List<string>();

                foreach (var converterFiles in appliedConverters)
                {
                    var converterFactory = converterFiles.Key;
                    if (converterFactory != null)
                    {
                        var converter = converterFactory(world);
                        ImportConfigurator.Spawn(assetClass, converterFiles, world, position, rotation, scale, converter);

                        position += rotation * float3.Forward;
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

        [HarmonyPatch(typeof(AssetHelper), "ClassifyExtension")]
        class ClassifyExtensionPatch
        {
            public static bool Prefix(ref AssetClass __result, string ext)
            {
                if (string.IsNullOrWhiteSpace(ext))
                    return true;
                return !AssetClassOverrides.TryGet(ext, out __result);
            }
        }
    }
}