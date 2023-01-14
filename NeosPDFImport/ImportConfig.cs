using NeosDocumentImport;
using System.Collections.Generic;
//using Ghostscript.NET.Rasterizer;

namespace NeosDocumentImport
{
    public struct ImportConfig
    {
        public int ppi;
        public bool opaque;
        public List<PageRange> pages;

        public ImportConfig(int dpi, bool opaque, List<PageRange> pages)
        {
            this.ppi = dpi;
            this.opaque = opaque;
            this.pages = pages;
        }

        public bool IsValid
        { 
            get
            {
                return ppi > 0 && pages != null;
            }
        }
    }
}