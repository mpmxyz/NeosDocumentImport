using CodeX;
using System.Collections.Generic;
using System.IO;

namespace NeosDocumentImport
{
    public static class Extensions
    {
        private static readonly Dictionary<string, AssetClass> overrides =
    new Dictionary<string, AssetClass>();

        public static void RegisterOverrides(AssetClass assetClass, params string[] extensions)
        {
            foreach (var extension in extensions)
            {
                overrides.Add(extension, assetClass);
            }
        }

        public static bool TryGet(string extension, out AssetClass assetClass)
        {
            extension = extension.Replace(".", "").ToLower();
            return overrides.TryGetValue(extension, out assetClass);
        }
    }
}
