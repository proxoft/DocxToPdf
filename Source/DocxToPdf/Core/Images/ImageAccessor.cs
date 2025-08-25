using System.IO;
using DocumentFormat.OpenXml.Packaging;

namespace Proxoft.DocxToPdf.Core.Images;

internal class ImageAccessor : IImageAccessor
{
    private readonly MainDocumentPart _mainDocumentPart;
    private ImageAccessor(MainDocumentPart mainDocumentPart)
    {
        _mainDocumentPart = mainDocumentPart;
    }

    public Stream GetImageStream(string imageId)
    {
        OpenXmlPart imagePart = _mainDocumentPart.GetPartById(imageId);
        Stream stream = imagePart.GetStream();
        return stream;
    }

    public static ImageAccessor Create(MainDocumentPart mainDocumentPart) =>
        new(mainDocumentPart);
}
