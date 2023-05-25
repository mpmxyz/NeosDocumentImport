using PdfLibCore;
using System;

namespace NeosDocumentImport_PDFConverter
{
    /// <summary>
    /// document related adapter to Docnet.Core library
    /// </summary>
    internal class DocumentHelper : IDisposable
    {
        private readonly PdfDocument doc;

        internal int pageCount
        {
            get { return doc.Pages.Count; }
        }

        public DocumentHelper(byte[] data, string password = null)
        {
            doc = new PdfDocument(data, password: password);
        }

        internal PageHelper GetPage(int iPage)
        {
            return new PageHelper(doc.Pages[iPage - 1]);
        }

        public void Dispose()
        {
            doc.Close();
        }
    }
}
