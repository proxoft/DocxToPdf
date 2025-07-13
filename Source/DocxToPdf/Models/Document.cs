using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using Proxoft.DocxToPdf.Core.Pages;
using Proxoft.DocxToPdf.Core.Rendering;
using Proxoft.DocxToPdf.Models.Common;
using Proxoft.DocxToPdf.Models.Sections;
using Proxoft.DocxToPdf.Models.Sections.Builders;
using Proxoft.DocxToPdf.Models.Styles.Services;

namespace Proxoft.DocxToPdf.Models;

internal class Document(WordprocessingDocument docx)
{
    private Section[] _sections = [];
    private readonly WordprocessingDocument _docx = docx;
    private readonly IStyleFactory _styleAccessor = StyleFactory.Default(docx.MainDocumentPart);

    public void Render(IRenderer renderer)
    {
        this.InitializeSections();

        this.PrepareSections();

        this.RenderSections(renderer);
    }

    private void InitializeSections()
    {
        _sections = _docx.MainDocumentPart.SplitToSections(_styleAccessor);
    }

    private void PrepareSections()
    {
        bool isFinished;
        var lastPageNumber = PageNumber.None;

        do
        {
            var previousSection = PageRegion.None;
            var previousSectionMargin = PageMargin.PageNone;

            foreach (var section in _sections)
            {
                section.Prepare(previousSection, previousSectionMargin, new DocumentVariables(lastPageNumber));
                previousSection = section.PageRegions.Last();
                previousSectionMargin = section.Pages.Last().Margin;
            }

            var secionLastPage = _sections.Last()
                .Pages
                .Last();

            isFinished = lastPageNumber == secionLastPage.PageNumber;
            lastPageNumber = secionLastPage.PageNumber;
        } while (!isFinished);
    }

    private void RenderSections(IRenderer renderer)
    {
        foreach(var section in _sections)
        {
            foreach(var page in section.Pages)
            {
                renderer.CreatePage(page.PageNumber, page.Configuration);
            }

            section.Render(renderer);
        }
    }
}
