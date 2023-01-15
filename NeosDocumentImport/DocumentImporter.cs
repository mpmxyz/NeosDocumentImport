using System.IO;
using BaseX;
using FrooxEngine;
using System.Collections.Generic;
using System;

namespace NeosDocumentImport
{
    public class DocumentImporter
    {
        public static void UpdateProgress(IProgressIndicator progress, string file, int iPage, int nPages)
        {
            float percent = (float)iPage / nPages;
            progress?.UpdateProgress(percent, $"Converting {file}", $"{iPage}/{nPages}");
        }

        public static void Spawn(IEnumerable<string> files, IConverter converter, ImportConfig config, World world, float3 position, floatQ rotation)
        {
            var slot = world.AddSlot("PDF Converter", false);
            var progress = slot.AttachComponent<NeosLogoMenuProgress>();
            progress.Spawn(position, 0.05f, true);

            world.RunInBackground(() =>
            {
                Import(files, converter, config, world, slot, progress);
            });
        }

        private static void Import(IEnumerable<string> files, IConverter converter, ImportConfig config, World world, Slot slot, NeosLogoMenuProgress progress)
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
                    var pages = converter.Apply(file, dir, prefix, config, progress);
                    world.RunSynchronously(() =>
                    {
                        BatchFolderImporter.BatchImport(slot.AddSlot(filename), pages);
                    });
                }
                catch (Exception e)
                {
                    progress.ProgressFail(
                        string.Format("Failed to convert file '{0}': {1}", filename, e.GetType().Name)
                    );
                    throw e;
                }
            }

            world.RunSynchronously(() =>
            {
                progress.Slot.Destroy();
            });
        }
    }
}