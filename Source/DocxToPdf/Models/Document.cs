using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Models.Common;
using Proxoft.DocxToPdf.Models.Sections;
using Proxoft.DocxToPdf.Models.Sections.Builders;
using Proxoft.DocxToPdf.Models.Styles;

namespace Proxoft.DocxToPdf.Models
{
    internal class Document
    {
        private Section[] _sections = new Section[0];
        private readonly WordprocessingDocument _docx;
        private readonly IStyleFactory _styleAccessor;

        public Document(WordprocessingDocument docx)
        {
            _docx = docx;
            _styleAccessor = StyleFactory.Default(docx.MainDocumentPart);
        }

        public void Render(IRenderer renderer)
        {
            this.InitializeSections();

            this.PrepareSections();

            this.RenderSections(renderer);
        }

        private void InitializeSections()
        {
            _sections = _docx.MainDocumentPart
                .SplitToSections(_styleAccessor)
                .ToArray();
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
}
