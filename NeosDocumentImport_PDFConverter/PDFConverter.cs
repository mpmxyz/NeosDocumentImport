using BaseX;
using FrooxEngine;
using NeosDocumentImport_PDFConverter;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NeosDocumentImport
{
    /// <summary>
    /// configures and applies conversion of pdf files to a list of image files
    /// </summary>
    internal class PDFConverter : IConverter
    {
        private const string PAGE_SUFFIX = ".png";

        [Range(50, 300, "0")]
        [Config("PPI", ConfigType.Value)]
        private int ppi = 150;
        [Config("Pages", ConfigType.Value)]
        private string rawPages = null;
        [Config("Password", ConfigType.Value, secret: true)]
        private string password = null;

        private List<PageRange> pages = new List<PageRange>();

        private readonly World world;

        public PDFConverter(World world)
        {
            this.world = world;
        }

        public bool ValidateConfig(out string msg)
        {
            msg = null;
            pages = PageRange.FromString(rawPages);

            if (ppi <= 0)
            {
                msg += "Invalid PPI\n";
            }
            if (pages == null)
            {
                msg += "Invalid Pages\n";
            }
            return msg == null;
        }

        public async Task<List<string>> Apply(string file, string outputDir, string pagePrefix, IProgressIndicator progress)
        {
            var filename = Path.GetFileName(file);

            progress?.Update(filename, 0, "Loading File...");

            var data = await ImportUtils.LoadData(file, world);

            progress?.Update(filename, 0, "Loading Data...");

            using (var helper = new DocumentHelper(data, password))
            {
                var nPages = helper.pageCount;
                var pageNumberLength = nPages.ToString().Length;

                var pages = this.pages;
                if (!pages.Any())
                {
                    pages = new List<PageRange> { new PageRange(1, nPages) };
                }

                pages = pages.ConvertAll((range) => range.Clamp(1, nPages));
                pages.RemoveAll((range) => range == null);

                var outputFiles = new List<string>();
                var nDone = 0;
                var nRequested = pages.Sum((x) => x.Count);

                float percentage() => (float)nDone / nRequested;
                string progText(int _nDone) => $"{filename} ({_nDone + 1}/{nRequested})";

                foreach (var range in pages)
                {
                    foreach (var iPage in range)
                    {
                        var pageNumber = iPage.ToString();
                        pageNumber = new string('0', pageNumberLength - pageNumber.Length) + pageNumber;
                        var outputFile = Path.Combine(outputDir, $"{pagePrefix}_{pageNumber}{PAGE_SUFFIX}");

                        if (!outputFiles.Contains(outputFile))
                        {
                            progress?.Update(progText(nDone), percentage(), "Loading... ");

                            using (var page = helper.GetPage(iPage))
                            {
                                progress?.Update(progText(nDone), percentage(), "Drawing...");

                                page.Render(ppi);

                                progress?.Update(progText(nDone), percentage(), "Saving...");

                                page.Save(outputFile);
                            }
                        }

                        outputFiles.Add(outputFile);
                        nDone++;

                        progress?.Update(progText(nDone - 1), percentage(), "Done!");
                    }
                }

                return outputFiles;
            }
        }

    }
}
