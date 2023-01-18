using FrooxEngine;
using System;
using System.IO;

namespace NeosDocumentImport
{
    public static class ImportUtils
    {
        public static byte[] LoadData(string file, World world)
        {
            //TODO: try to find way to reuse proper asset handling methods (->neosdb/local)
            if (Uri.IsWellFormedUriString(file, UriKind.Absolute))
            {
                var download = world.Engine.Cloud.SafeHttpClient.GetByteArrayAsync(file);
                download.Wait();
                return download.Result;
            }
            else
            {
                return File.ReadAllBytes(file);
            }
        }
    }
}
