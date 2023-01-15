using BaseX;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NeosDocumentImport
{
    internal class HTMLConverter : IConverter
    {
        public List<string> Apply(string file, string outputDir, string pagePrefix, ImportConfig config, IProgressIndicator progress)
        {
            var outputFiles = new List<string>();
            using (var browserFetcher = new BrowserFetcher())
            {
                var task = Task.Run(async () =>
                {
                    await browserFetcher.DownloadAsync();
                    var launchOptions = new LaunchOptions { Headless = true };
                    using (var browser = await Puppeteer.LaunchAsync(launchOptions))
                    {
                        using (var page = await browser.NewPageAsync())
                        {
                            var outputFile = Path.Combine(outputDir, $"{pagePrefix}.png");

                            await page.SetViewportAsync(new ViewPortOptions
                            {
                                Width = config.ppi * 12, Height = config.ppi * 12,
                            });
                            if (!Uri.IsWellFormedUriString(file, UriKind.Absolute))
                            {
                                file = $"file://{file}";
                            }
                            await page.GoToAsync(file);
                            Directory.CreateDirectory(outputDir);

                            var options = new ScreenshotOptions
                            {
                                FullPage = true,
                                OmitBackground = !config.opaque
                            };
                            await page.ScreenshotAsync(outputFile, options);
                            outputFiles.Add(outputFile);
                        }
                    }
                });
                task.Wait();
            }
            return outputFiles;
        }
    }
}
