using System.IO;
using BaseX;
using FrooxEngine;
using System.Collections.Generic;
using System;

namespace NeosDocumentImport
{
    public class DocumentImporter
    {
        public static void UpdateProgress(IProgressIndicator progress, string name, float percent, string details)
        {
            progress?.UpdateProgress(percent, $"Converting {name}", details);
        }

        public static void Spawn(IEnumerable<string> files, IConverter converter, World world, float3 position, floatQ rotation)
        {
            var slot = world.AddSlot("File Converter", false);
            var progress = slot.AttachComponent<NeosLogoMenuProgress>();
            progress.Spawn(position, 0.05f, true);

            world.RunInBackground(() =>
            {
                Import(files, converter, world, position, rotation, progress);
                world.RunSynchronously(() =>
                {
                    slot.Destroy();
                });
            });
        }

        private static async void Import(IEnumerable<string> files, IConverter converter, World world, float3 position, floatQ rotation, NeosLogoMenuProgress progress)
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
                        string.Format("Failed to convert file '{0}': {1}", filename, e.GetType().Name)
                    );
                    throw e;
                }
            }
        }
    }
}