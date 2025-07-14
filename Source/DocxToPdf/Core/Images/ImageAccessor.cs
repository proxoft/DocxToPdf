using System.IO;
using DocumentFormat.OpenXml.Packaging;

namespace Proxoft.DocxToPdf.Core.Images;

internal class ImageAccessor(MainDocumentPart mainDocumentPart) : IImageAccessor
{
    private readonly MainDocumentPart _mainDocumentPart = mainDocumentPart;

    public Stream GetImageStream(string imageId)
    {
        OpenXmlPart imagePart = _mainDocumentPart.GetPartById(imageId);
        Stream stream = imagePart.GetStream();
        return stream;
    }
}
