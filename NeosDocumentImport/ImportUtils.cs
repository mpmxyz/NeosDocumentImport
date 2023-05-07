using BaseX;
using FrooxEngine;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NeosDocumentImport
{
    /// <summary>
    /// A collection of utility functions that are likely used by all converter implementations
    /// </summary>
    public static class ImportUtils
    {
        /// <summary>
        /// Loads the given file into memory
        /// </summary>
        /// <param name="uri">Uri of the file to be loaded</param>
        /// <param name="world">World that the file will be loaded in</param>
        /// <returns>A byte array with the file contents</returns>
        public static async Task<byte[]> LoadData(string uri, World world)
        {
            uri = await RequestFile(uri, world);

            await new ToBackground();

            return File.ReadAllBytes(uri);
        }

        /// <summary>
        /// Downloads
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="world"></param>
        /// <returns></returns>
        public static async Task<string> RequestFile(string uri, World world)
        {
            if (Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            {
                return await world.Engine.AssetManager.RequestGather(new Uri(uri), Priority.Normal);
            }
            else
            {
                return uri;
            }
        }


        public static void Update(this IProgressIndicator progress, string name, float percent, string details)
        {
            progress.UpdateProgress(percent, $"Converting {name}", details);
        }
    }
}
