using BaseX;
using CodeX;
using System.Collections.Generic;
using System.IO;

namespace NeosDocumentImport
{
    public interface IConverter
    {
        List<string> Apply(string file, string outputDir, string pagePrefix, ImportConfig config, IProgressIndicator progress);
    }

    public static class Converters
    {
        private static readonly Dictionary<(AssetClass, string), IConverter> converters =
    new Dictionary<(AssetClass, string), IConverter>();

        public static void Register(IConverter converter, AssetClass assetClass, params string[] extensions)
        {
            foreach (var extension in extensions)
            {
                converters.Add((assetClass, extension), converter);
            }
        }

        public static IConverter Get(AssetClass assetClass, string file)
        {
            var extension = Path.GetExtension(file).ToLower();
            UniLog.Log($"{assetClass} {file} {extension}");
            UniLog.Log($"{converters}");
            return converters.TryGetValue((assetClass, extension), out var converter) ? converter : null;
        }
    }
}
