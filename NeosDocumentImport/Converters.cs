using BaseX;
using CodeX;
using FrooxEngine;
using System;
using System.Collections.Generic;
using System.IO;

namespace NeosDocumentImport
{
    public static class Converters
    {
        private static readonly Dictionary<(AssetClass, string), Func<World, IConverter>> factories =
    new Dictionary<(AssetClass, string), Func<World, IConverter>>();

        public static void Register(Func<World, IConverter> factory, AssetClass assetClass, params string[] extensions)
        {
            foreach (var extension in extensions)
            {
                factories.Add((assetClass, extension), factory);
            }
        }

        public static Func<World, IConverter> GetFactory(AssetClass assetClass, string file)
        {
            var extension = Path.GetExtension(file).Replace(".", "").ToLower();
            UniLog.Log($"assetClass: {assetClass}, ext: {extension}");
            return factories.TryGetValue((assetClass, extension), out var factory) ? factory : null;
        }
    }
}
