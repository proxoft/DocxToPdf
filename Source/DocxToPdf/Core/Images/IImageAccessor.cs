using System.IO;

namespace Proxoft.DocxToPdf.Core.Images;

internal interface IImageAccessor
{
    Stream GetImageStream(string imageId);
}
