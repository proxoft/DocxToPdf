using System;

namespace Proxoft.DocxToPdf
{
    public class RendererException : Exception
    {
        public RendererException(string message) : base(message)
        {
        }
    }
}
