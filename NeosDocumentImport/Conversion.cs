using System.IO;
using BaseX;
using FrooxEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace NeosDocumentImport
{
    /// <summary>
    /// Helper class to start a batch conversion for multiple files (includes a progress indicator)
    /// </summary>
    internal static class Conversion
    {
        internal static void Start(IEnumerable<string> files, IConverter converter, World world, float3 position, floatQ rotation)
        {
            var slot = world.AddSlot("File Converter", false);
            var progress = slot.AttachComponent<NeosLogoMenuProgress>();
            progress.Spawn(position, 0.05f, true);

            world.RunInBackground(async () =>
            {
                await Convert(files, converter, world, position, rotation, progress);
                world.RunSynchronously(() => slot.Destroy());
            });
        }

        private static async Task Convert(IEnumerable<string> files, IConverter converter, World world, float3 position, floatQ rotation, NeosLogoMenuProgress progress)
        {
            var localDb = world.Engine.LocalDB;
            var imageDirs = new List<string>();

            foreach (var file in files)
            {
                var filename = Path.GetFileName(file);
                var dir = localDb.GetTempFilePath();
                var prefix = Path.GetFileNameWithoutExtension(file);
                imageDirs.Add(dir);

                try
                {
                    Directory.CreateDirectory(dir);
                    var pages = await converter.Apply(file, dir, prefix, progress);
                    world.RunSynchronously(() =>
                    {
                        var slot = world.AddSlot(filename, false);
                        slot.GlobalPosition = position;
                        slot.GlobalRotation = rotation;

                        position += rotation * float3.Forward;

                        BatchFolderImporter.BatchImport(slot, pages); //destroys slot
                    });
                }
                catch (Exception e)
                {
                    progress.ProgressFail(
                        string.Format("Failed to convert file '{0}': {1}", filename, e.Message)
                    );
                    throw e;
                }
            }
        }
    }
}