using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Генератор DOCX-уведомления ГД об увольнении (ст. 280 ТК РФ) в формате ГОСТ.
/// Times New Roman 14pt, поля: левое 30мм, правое 15мм, верх/низ 20мм, интервал 1.5.
/// </summary>
public class CeoResignationDocxGenerator : ICeoResignationDocxGenerator
{
    private const string FontName = "Times New Roman";
    private const int FontSizePt = 14;
    private const int LeftMarginMm = 30;
    private const int RightMarginMm = 15;
    private const int TopMarginMm = 20;
    private const int BottomMarginMm = 20;
    private const int LineSpacing15 = 360;

    private static int MmToTwips(int mm) => (int)(mm * 56.692913);
    private static int PtToHalfPt(int pt) => pt * 2;

    public Task<byte[]> GenerateAsync(CeoResignationData data, CancellationToken ct = default)
    {
        using var stream = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
        {
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            var sectionProps = new SectionProperties(
                new PageSize { Width = 11906, Height = 16838 },
                new PageMargin
                {
                    Left = (uint)MmToTwips(LeftMarginMm),
                    Right = (uint)MmToTwips(RightMarginMm),
                    Top = MmToTwips(TopMarginMm),
                    Bottom = MmToTwips(BottomMarginMm),
                });
            body.AppendChild(sectionProps);

            AddStyles(mainPart);
            AddNotification(body, data);
        }

        return Task.FromResult(stream.ToArray());
    }

    private static void AddStyles(MainDocumentPart mainPart)
    {
        var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
        var styles = new Styles();

        var normalStyle = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = "Normal",
            Default = true
        };
        normalStyle.AppendChild(new StyleName { Val = "Normal" });
        normalStyle.AppendChild(new StyleParagraphProperties(
            new SpacingBetweenLines { Line = LineSpacing15.ToString(), LineRule = LineSpacingRuleValues.Auto }));
        normalStyle.AppendChild(new StyleRunProperties(
            new RunFonts { Ascii = FontName, HighAnsi = FontName, ComplexScript = FontName },
            new FontSize { Val = PtToHalfPt(FontSizePt).ToString() }));
        styles.AppendChild(normalStyle);

        stylesPart.Styles = styles;
    }

    private static Paragraph CreateParagraph(string text, bool bold = false, bool center = false,
        int? spaceAfterPt = null)
    {
        var para = new Paragraph();
        var pPr = new ParagraphProperties();

        if (center)
            pPr.AppendChild(new Justification { Val = JustificationValues.Center });

        if (spaceAfterPt.HasValue)
            pPr.AppendChild(new SpacingBetweenLines { After = (spaceAfterPt.Value * 20).ToString() });

        para.AppendChild(pPr);

        var run = new Run();
        var rPr = new RunProperties(
            new RunFonts { Ascii = FontName, HighAnsi = FontName, ComplexScript = FontName },
            new FontSize { Val = PtToHalfPt(FontSizePt).ToString() });
        if (bold)
            rPr.AppendChild(new Bold());
        run.AppendChild(rPr);
        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        para.AppendChild(run);

        return para;
    }

    private static void AddNotification(Body body, CeoResignationData data)
    {
        // Блок адресата
        body.AppendChild(CreateParagraph($"Участнику {data.LegalEntityName}"));
        body.AppendChild(CreateParagraph(data.ParticipantFullName));
        if (!string.IsNullOrWhiteSpace(data.ParticipantAddress))
            body.AppendChild(CreateParagraph($"Адрес: {data.ParticipantAddress}"));

        body.AppendChild(new Paragraph());

        // Заголовок
        body.AppendChild(CreateParagraph("УВЕДОМЛЕНИЕ", bold: true, center: true, spaceAfterPt: 6));
        body.AppendChild(CreateParagraph("о предстоящем увольнении Генерального директора",
            bold: true, center: true, spaceAfterPt: 12));

        // Вводный абзац
        var resignationDateStr = $"{data.ResignationDate.Day} {FormatMonth(data.ResignationDate.Month)} {data.ResignationDate.Year}";
        var notificationDateStr = $"{data.NotificationDate.Day} {FormatMonth(data.NotificationDate.Month)} {data.NotificationDate.Year}";

        var introText = $"Настоящим {data.LegalEntityName} (ОГРН {data.LegalEntityOgrn ?? "___"}, ИНН {data.LegalEntityInn ?? "___"}) " +
                        $"уведомляет Вас о том, что Генеральный директор {data.CeoName} " +
                        $"уведомил общество о своём предстоящем увольнении в соответствии со статьёй 280 " +
                        $"Трудового кодекса Российской Федерации.";
        body.AppendChild(CreateParagraph(introText, spaceAfterPt: 12));

        // Детали
        body.AppendChild(CreateParagraph($"Плановая дата увольнения: {resignationDateStr}", spaceAfterPt: 6));
        body.AppendChild(CreateParagraph($"Дата уведомления: {notificationDateStr}", spaceAfterPt: 12));

        // Юридическое основание
        body.AppendChild(CreateParagraph(
            "В соответствии со статьёй 280 ТК РФ руководитель организации обязан уведомить " +
            "работодателя (учредителей) не позднее чем за один месяц до даты предстоящего увольнения.",
            spaceAfterPt: 12));

        // Созыв ВОСУ
        body.AppendChild(CreateParagraph(
            "Для избрания нового Генерального директора общества созывается " +
            "внеочередное общее собрание участников (ВОСУ).",
            bold: true, spaceAfterPt: 12));

        // Повестка ВОСУ
        if (data.AgendaItems.Count > 0)
        {
            body.AppendChild(CreateParagraph("ПОВЕСТКА ДНЯ ВОСУ:", bold: true, spaceAfterPt: 6));
            for (int i = 0; i < data.AgendaItems.Count; i++)
            {
                body.AppendChild(CreateParagraph($"{i + 1}. {data.AgendaItems[i]}", spaceAfterPt: 3));
            }
            body.AppendChild(new Paragraph());
        }

        // Ознакомление
        var reviewText = string.IsNullOrWhiteSpace(data.ReviewLocation)
            ? "Ознакомиться с дополнительными материалами можно в рабочие дни с 10:00 до 16:00."
            : $"Ознакомиться с дополнительными материалами можно по адресу: {data.ReviewLocation}.";
        body.AppendChild(CreateParagraph(reviewText, spaceAfterPt: 18));

        // Подпись
        body.AppendChild(CreateParagraph(
            $"Генеральный директор {data.LegalEntityName}  ________________ / {data.CeoName}/",
            spaceAfterPt: 6));
        body.AppendChild(CreateParagraph(
            $"«{data.NotificationDate.Day}» {FormatMonth(data.NotificationDate.Month)} {data.NotificationDate.Year} года"));
    }

    private static string FormatMonth(int month) => month switch
    {
        1 => "января",
        2 => "февраля",
        3 => "марта",
        4 => "апреля",
        5 => "мая",
        6 => "июня",
        7 => "июля",
        8 => "августа",
        9 => "сентября",
        10 => "октября",
        11 => "ноября",
        12 => "декабря",
        _ => ""
    };
}
