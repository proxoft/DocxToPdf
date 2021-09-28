using System.IO;

namespace Proxoft.DocxToPdf.Core
{
    internal interface IImageAccessor
    {
        Stream GetImageStream(string imageId);
    }
}
