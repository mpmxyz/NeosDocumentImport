using CodeX;
using System.Collections.Generic;

namespace NeosDocumentImport
{
    /// <summary>
    /// A registry of file extension based asset class overrides
    /// </summary>
    public static class AssetClassOverrides
    {
        private static readonly Dictionary<string, AssetClass> overrides =
    new Dictionary<string, AssetClass>();

        public static void Register(AssetClass assetClass, params string[] extensions)
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
