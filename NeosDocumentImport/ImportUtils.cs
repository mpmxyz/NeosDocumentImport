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
        /// Downloads to a temporary file if given an absolute URI
        /// </summary>
        /// <param name="uri">Uri of the file to be loaded</param>
        /// <param name="world">World that the file will be loaded in</param>
        /// <returns>The path to the downloaded file or <paramref name="uri"/> if no download has been triggered</returns>
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

        /// <summary>
        /// Updates the given progress indicator,
        /// mostly a convenience wrapper for <see cref="IProgressIndicator.UpdateProgress(float, string, string)"/>
        /// </summary>
        /// <param name="progress">To be updated</param>
        /// <param name="name">Is prefixed by "Converting "</param>
        /// <param name="percent">Unchanged</param>
        /// <param name="details">Unchanged</param>
        public static void Update(this IProgressIndicator progress, string name, float percent, string details)
        {
            progress.UpdateProgress(percent, $"Converting {name}", details);
        }
    }
}
