using FrooxEngine;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NeosDocumentImport
{
    public static class ImportUtils
    {
        public static async Task<byte[]> LoadData(string file, World world)
        {
            //TODO: try to find way to reuse proper asset handling methods (->neosdb/local)
            if (Uri.IsWellFormedUriString(file, UriKind.Absolute))
            {
                file = await world.Engine.AssetManager.RequestGather(new Uri(file), Priority.Normal);
            }
            return File.ReadAllBytes(file);
        }
    }
}
