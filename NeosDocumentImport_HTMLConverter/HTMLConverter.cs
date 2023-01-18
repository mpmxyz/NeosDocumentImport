using BaseX;
using FrooxEngine;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NeosDocumentImport
{
    internal class HTMLConverter : IConverter
    {
        [Range(100, 4096, "0")]
        [Config("Width", ConfigType.Value)]
        private int width = 1024;
        [Range(0.1f, 10f)]
        [Config("Display Pixel Ratio", ConfigType.Value)]
        private float dpr = 1f;
        [Config("Background", ConfigType.Value)]
        private bool background = true;


        private readonly Task<RevisionInfo> browserDownload;
        internal HTMLConverter(Task<RevisionInfo> browserDownload)
        {
            this.browserDownload = browserDownload;
        }

        public List<string> Apply(string file, string outputDir, string pagePrefix, IProgressIndicator progress)
        {
            var outputFiles = new List<string>();
            var task = Task.Run(async () =>
            {
                progress.UpdateProgress(0, $"Importing ${Path.GetFileName(file)}", "Download Browser...");

                var launchOptions = new LaunchOptions {
                    Headless = true,
                    ExecutablePath = (await browserDownload).ExecutablePath,
                    Args = new string[]{ "--disable-lcd-text" }
                };

                progress.UpdateProgress(1/5f, $"Importing ${Path.GetFileName(file)}", "Launching Browser...");

                using (var browser = await Puppeteer.LaunchAsync(launchOptions))
                {
                    progress.UpdateProgress(2/5f, $"Importing ${Path.GetFileName(file)}", "Open Tab...");

                    using (var page = await browser.NewPageAsync())
                    {
                        var outputFile = Path.Combine(outputDir, $"{pagePrefix}.png");

                        await page.SetViewportAsync(new ViewPortOptions
                        {
                            Width = width, Height = 512, DeviceScaleFactor = dpr
                        });
                        if (!Uri.IsWellFormedUriString(file, UriKind.Absolute))
                        {
                            file = $"file://{file}";
                        }

                        progress.UpdateProgress(3/5f, $"Importing ${Path.GetFileName(file)}", "Loading File...");
                        await page.GoToAsync(file);

                        progress.UpdateProgress(4/5f, $"Importing ${Path.GetFileName(file)}", "Rendering/Writing File...");
                        var options = new ScreenshotOptions
                        {
                            FullPage = true,
                            OmitBackground = !background
                        };
                        await page.ScreenshotAsync(outputFile, options);

                        progress.UpdateProgress(1, $"Importing ${Path.GetFileName(file)}", "Done!");
                        outputFiles.Add(outputFile);
                    }
                }
            });
            task.Wait();
            return outputFiles;
        }

        public bool ValidateConfig()
        {
            return width > 0;
        }
    }
}
