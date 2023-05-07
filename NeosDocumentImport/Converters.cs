using CodeX;
using FrooxEngine;
using System;
using System.Collections.Generic;
using System.IO;

namespace NeosDocumentImport
{
    /// <summary>
    /// A public registry of converters, identified via a combination of asset class and file extension
    /// </summary>
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
            return factories.TryGetValue((assetClass, extension), out var factory) ? factory : null;
        }
    }
}
