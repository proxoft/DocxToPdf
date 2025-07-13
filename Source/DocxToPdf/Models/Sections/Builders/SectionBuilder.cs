using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Core.Images;
using Proxoft.DocxToPdf.Extensions;
using Proxoft.DocxToPdf.Extensions.Units;
using Proxoft.DocxToPdf.Models.Common;
using Proxoft.DocxToPdf.Models.Core;
using Proxoft.DocxToPdf.Models.Sections.Columns;
using Proxoft.DocxToPdf.Models.Styles.Services;
using OpenXml = DocumentFormat.OpenXml;
using Pack = DocumentFormat.OpenXml.Packaging;

namespace Proxoft.DocxToPdf.Models.Sections.Builders;

internal static class SectionBuilder
{
    public static Section[] SplitToSections(
        this Pack.MainDocumentPart? mainDocument,
        IStyleFactory styleFactory)
    {
        if (mainDocument?.Document.Body is null)
        {
            return [];
        }

        Section[] sections = mainDocument.Document.Body
            .SplitToSectionsCore(mainDocument, styleFactory);

        return sections;
    }

    private static Section[] SplitToSectionsCore(
        this Word.Body body,
        Pack.MainDocumentPart mainDocumentPart,
        IStyleFactory styleFactory)
    {
        if (body is null)
        {
            return [];
        }

        Section[] sections = [];

        List<OpenXml.OpenXmlCompositeElement> sectionElements = [];
        HeaderFooterConfiguration headerFooterConfiguration = HeaderFooterConfiguration.Empty;
        Word.SectionProperties? wordSectionProperties;

        foreach (OpenXml.OpenXmlCompositeElement e in body.RenderableChildren())
        {
            sectionElements.Add(e);
            if (e is not Word.Paragraph paragraph)
            {
                continue;
            }

            wordSectionProperties = paragraph.GetSectionProperties();
            if (wordSectionProperties == null)
            {
                continue;
            }

            Section section = sectionElements.CreateSection(wordSectionProperties, mainDocumentPart, headerFooterConfiguration, sections.Length == 0, styleFactory);
            headerFooterConfiguration = section.HeaderFooterConfiguration;
            sections = [
                ..sections,
                section
            ];

            sectionElements.Clear();
        }

        wordSectionProperties = body
           .ChildsOfType<Word.SectionProperties>()
           .Single();

        Section lastSection = sectionElements.CreateSection(wordSectionProperties, mainDocumentPart, headerFooterConfiguration, sections.Length == 0, styleFactory);
        sections = [
            ..sections,
            lastSection
        ];
        return sections;
    }

    private static Section CreateSection(
        this IReadOnlyCollection<OpenXml.OpenXmlCompositeElement> xmlElements,
        Word.SectionProperties wordSectionProperties,
        Pack.MainDocumentPart mainDocumentPart,
        HeaderFooterConfiguration headerFooterConfiguration,
        bool isFirst,
        IStyleFactory styleFactory)
    {
        ImageAccessor imageAccessor = new(mainDocumentPart);

        SectionProperties sectionProperties = wordSectionProperties.CreateSectionProperties(mainDocumentPart, isFirst, headerFooterConfiguration);
        ColumnsConfiguration columnsConfiguration = wordSectionProperties.CreateColumnsConfiguration(sectionProperties.PageConfiguration, sectionProperties.Margin);
        SectionContent[] sectionContents = xmlElements.SplitToSectionContents(columnsConfiguration, imageAccessor, styleFactory);
        Section sd = new(sectionProperties, sectionContents, imageAccessor, styleFactory);

        return sd;
    }

    private static SectionContent[] SplitToSectionContents(
        this IEnumerable<OpenXml.OpenXmlCompositeElement> xmlElements,
        ColumnsConfiguration columnsConfiguration,
        IImageAccessor imageAccessor,
        IStyleFactory styleFactory)
    {
        List<SectionContent> sectionContents = [];

        var stack = xmlElements.ToStackReversed();
        var contentElements = new List<OpenXml.OpenXmlCompositeElement>();

        while (stack.Count > 0)
        {
            var e = stack.Pop();
            switch (e)
            {
                case Word.Paragraph p:
                    {
                        (Word.Paragraph begin, SectionContentBreak @break, Word.Paragraph? end) = p.SplitByNextBreak();
                        if (@break == SectionContentBreak.None)
                        {
                            contentElements.Add(p);
                        }
                        else
                        {
                            if (end is not null)
                            {
                                stack.Push(end);
                            }

                            contentElements.Add(begin);
                            PageContextElement[] childElements = contentElements.CreatePageElements(imageAccessor, styleFactory);
                            sectionContents.Add(new SectionContent(childElements, columnsConfiguration, @break));
                            contentElements.Clear();
                        }
                    }
                    break;
                default:
                    contentElements.Add(e);
                    break;
            }
        }

        if (contentElements.Count > 0)
        {
            PageContextElement[] childElements = contentElements.CreatePageElements(imageAccessor, styleFactory);
            sectionContents.Add(new SectionContent(childElements, columnsConfiguration, SectionContentBreak.None));
        }

        return [.. sectionContents];
    }

    private static Word.SectionProperties? GetSectionProperties(this Word.Paragraph paragraph) =>
        paragraph.ParagraphProperties?.SectionProperties;

    private static SectionProperties CreateSectionProperties(
        this Word.SectionProperties wordSectionProperties,
        Pack.MainDocumentPart mainDocument,
        bool isFirstSection,
        HeaderFooterConfiguration inheritHeaderFooterConfiguration)
    {
        var pageCongifuration = wordSectionProperties.GetPageConfiguration();
        var pageMargin = wordSectionProperties.GetPageMargin();

        var sectionMark = wordSectionProperties.ChildsOfType<Word.SectionType>().SingleOrDefault()?.Val ?? Word.SectionMarkValues.NextPage;

        var requiresNewPage = isFirstSection || sectionMark == Word.SectionMarkValues.NextPage;

        var headerFooterConfiguration = wordSectionProperties
            .GetHeaderFooterConfiguration(mainDocument, inheritHeaderFooterConfiguration);

        return new SectionProperties(
            pageCongifuration,
            headerFooterConfiguration,
            pageMargin,
            requiresNewPage);
    }

    private static PageConfiguration GetPageConfiguration(
        this Word.SectionProperties sectionProperties)
    {
        var pageSize = sectionProperties.ChildsOfType<Word.PageSize>().Single();
        var w = pageSize.Width.DxaToPoint();
        var h = pageSize.Height.DxaToPoint();


        var orientation = (pageSize.Orient?.Value ?? Word.PageOrientationValues.Portrait) == Word.PageOrientationValues.Portrait
            ? PageOrientation.Portrait
            : PageOrientation.Landscape;

        return new PageConfiguration(new Size(w, h), orientation);
    }

    private static PageMargin GetPageMargin(this Word.SectionProperties sectionProperties)
    {
        Word.PageMargin pageMargin = sectionProperties.ChildsOfType<Word.PageMargin>().Single();

        return new PageMargin(
            pageMargin.Top.DxaToPoint(),
            pageMargin.Right.DxaToPoint(),
            pageMargin.Bottom.DxaToPoint(),
            pageMargin.Left.DxaToPoint(),
            pageMargin.Header.DxaToPoint(),
            pageMargin.Footer.DxaToPoint());
    }

    private static HeaderFooterConfiguration GetHeaderFooterConfiguration(
        this Word.SectionProperties wordSectionProperties,
        Pack.MainDocumentPart mainDocument,
        HeaderFooterConfiguration previousHeaderFooterConfiguration)
    {
        bool hasTitlePage = wordSectionProperties.ChildsOfType<Word.TitlePage>().SingleOrDefault()
              .IsOn(ifOnOffTypeNull: false, ifOnOffValueNull: true);

        HeaderFooterRef[] headerRefs = [.. wordSectionProperties
            .ChildsOfType<Word.HeaderReference>()
            .Where(fr => fr.Id is not null && fr.Type is not null)
            .Select(fr => new HeaderFooterRef(fr.Id!, fr.Type!))];

        HeaderFooterRef[] footerRefs = [.. wordSectionProperties
            .ChildsOfType<Word.FooterReference>()
            .Where(fr => fr.Id is not null && fr.Type is not null)
            .Select(fr => new HeaderFooterRef(fr.Id!, fr.Type!))];

        return previousHeaderFooterConfiguration.Inherited(mainDocument, hasTitlePage, headerRefs, footerRefs);
    }

    private static (Word.Paragraph, SectionContentBreak, Word.Paragraph?) SplitByNextBreak(this Word.Paragraph paragraph)
    {
        var index = paragraph
            .ChildElements
            .IndexOf(e => e is Word.Run r && r.ChildsOfType<Word.Break>().Any(b => b.Type != null && (b.Type == Word.BreakValues.Page || b.Type == Word.BreakValues.Column)));

        if (index == -1)
        {
            return (paragraph, SectionContentBreak.None, null);
        }

        var beginElements = paragraph
            .ChildElements
            .Take(index + 1)
            .Select(r => r.CloneNode(true))
            .ToArray();

        Word.Paragraph begin = new(beginElements)
        {
            ParagraphProperties = paragraph.ParagraphProperties?.CloneNode(true) as Word.ParagraphProperties
        };

        var endElements = paragraph
            .ChildElements
            .Skip(index + 1)
            .Select(r => r.CloneNode(true))
            .ToArray();

        Word.Paragraph? end = endElements.Length == 0
            ? null
            : new(endElements)
            {
                ParagraphProperties = paragraph.ParagraphProperties?.CloneNode(true) as Word.ParagraphProperties
            };

        var breakRun = (Word.Run)paragraph
            .ChildElements
            .ElementAt(index);

        var breakType = breakRun
            .ChildElements
            .OfType<Word.Break>()
            .Single()
            .Type;

        SectionContentBreak @break = breakType?.Value.ToSectionBreak() ?? SectionContentBreak.None;

        //var @break = SectionContentBreak.None;
        //switch (breakType.Value)
        //{
        //    case Word.BreakValues.Column:
        //        @break = SectionContentBreak.Column;
        //        break;

        //    case Word.BreakValues.Page:
        //        @break = SectionContentBreak.Page;
        //        break;
        //    default:
        //        throw new System.Exception("Unexpected value");
        //};

        return (begin, @break, end);
    }

    private static SectionContentBreak ToSectionBreak(this Word.BreakValues breakValue)
    {
        if (breakValue == Word.BreakValues.Column) return SectionContentBreak.Column;
        if (breakValue == Word.BreakValues.Page) return SectionContentBreak.Page;

        return SectionContentBreak.None;
    }
}
