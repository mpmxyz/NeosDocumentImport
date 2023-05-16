using Docnet.Core.Models;
using Docnet.Core;
using System;
using Docnet.Core.Readers;

namespace NeosDocumentImport_PDFConverter
{
    /// <summary>
    /// document related adapter to Docnet.Core library
    /// </summary>
    internal class DocumentHelper : IDisposable
    {
        private readonly IDocReader doc;
        internal int pageCount
        {
            get { return doc.GetPageCount(); }
        }

        public DocumentHelper(byte[] data, int ppi)
        {
            var dimensions = new PageDimensions(ppi / 72.0);
            doc = DocLib.Instance.GetDocReader(data, dimensions);
        }

        internal PageHelper GetPage(int iPage)
        {
            return new PageHelper(doc.GetPageReader(iPage - 1));
        }

        public void Dispose()
        {
            doc.Dispose();
        }
    }
}
