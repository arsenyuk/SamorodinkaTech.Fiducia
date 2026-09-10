using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Генератор DOCX-уведомления о проведении ООСУ (очередное собрание) в формате ГОСТ.
/// Times New Roman 14pt, поля: левое 30мм, правое 15мм, верх/низ 20мм, интервал 1.5.
/// </summary>
public class OosuNotificationDocxGenerator : IOosuNotificationDocxGenerator
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

    public Task<byte[]> GenerateAsync(OosuNotificationData data, CancellationToken ct = default)
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

    private static void AddNotification(Body body, OosuNotificationData data)
    {
        var leInfo = BuildLegalEntityInfo(data);

        body.AppendChild(CreateParagraph(leInfo, spaceAfterPt: 12));
        body.AppendChild(CreateParagraph("УВЕДОМЛЕНИЕ", bold: true, center: true, spaceAfterPt: 6));
        body.AppendChild(CreateParagraph("о проведении очередного общего собрания участников",
            bold: true, center: true, spaceAfterPt: 12));

        var introText = BuildIntroText(data);
        body.AppendChild(CreateParagraph(introText, spaceAfterPt: 12));

        AddMeetingDetails(body, data);
        AddAgendaSection(body, data);
        AddReviewSection(body, data);
        AddSignature(body, data);
    }

    private static string BuildLegalEntityInfo(OosuNotificationData data)
    {
        var parts = new List<string> { data.LegalEntityName };
        if (!string.IsNullOrWhiteSpace(data.LegalEntityOgrn))
            parts.Add($"ОГРН {data.LegalEntityOgrn}");
        if (!string.IsNullOrWhiteSpace(data.LegalEntityInn))
            parts.Add($"ИНН {data.LegalEntityInn}");
        return string.Join(", ", parts);
    }

    private static string BuildIntroText(OosuNotificationData data)
    {
        var yearText = data.FinancialYear.HasValue
            ? $" за {data.FinancialYear.Value} финансовый год"
            : "";

        return $"Настоящим {data.LegalEntityName} (ОГРН {data.LegalEntityOgrn ?? "___"}, ИНН {data.LegalEntityInn ?? "___"}) " +
               $"извещает Вас о проведении очередного общего собрания участников{yearText} " +
               $"в соответствии со статьями 34 и 36 Федерального закона № 14-ФЗ «Об обществах с ограниченной ответственностью».";
    }

    private static void AddMeetingDetails(Body body, OosuNotificationData data)
    {
        body.AppendChild(CreateParagraph("Очередное общее собрание участников состоится:", bold: true, spaceAfterPt: 6));

        var lines = new List<string>
        {
            $"• Дата проведения: {data.MeetingDate.Day} {FormatMonth(data.MeetingDate.Month)} {data.MeetingDate.Year} года"
        };

        if (data.MeetingStartTime.HasValue)
            lines.Add($"• Время начала: {data.MeetingStartTime.Value.Hour} часов {data.MeetingStartTime.Value.Minute} минут");

        if (!string.IsNullOrWhiteSpace(data.MeetingVenue))
            lines.Add($"• Место проведения: {data.MeetingVenue}");

        if (data.RegistrationStartTime.HasValue)
            lines.Add($"• Время начала регистрации участников: {data.RegistrationStartTime.Value.Hour} часов {data.RegistrationStartTime.Value.Minute} минут");

        foreach (var line in lines)
            body.AppendChild(CreateParagraph(line, spaceAfterPt: 3));
    }

    private static void AddAgendaSection(Body body, OosuNotificationData data)
    {
        body.AppendChild(new Paragraph());
        body.AppendChild(CreateParagraph("ПОВЕСТКА ДНЯ ООСУ:", bold: true, spaceAfterPt: 6));

        for (int i = 0; i < data.AgendaItems.Count; i++)
        {
            var itemText = $"{i + 1}. {data.AgendaItems[i]}";
            body.AppendChild(CreateParagraph(itemText, spaceAfterPt: 3));
        }
    }

    private static void AddReviewSection(Body body, OosuNotificationData data)
    {
        body.AppendChild(new Paragraph());
        var reviewText = string.IsNullOrWhiteSpace(data.ReviewLocation)
            ? "Ознакомиться с материалами и документами можно в рабочие дни с 10:00 до 16:00."
            : $"Ознакомиться с материалами и документами можно по адресу: {data.ReviewLocation}.";
        body.AppendChild(CreateParagraph(reviewText, spaceAfterPt: 18));
    }

    private static void AddSignature(Body body, OosuNotificationData data)
    {
        var signLine = $"Генеральный директор {data.LegalEntityName}  ________________ / {data.CeoName}/";
        var dateLine = $"«{data.NotificationDate.Day}» {FormatMonth(data.NotificationDate.Month)} {data.NotificationDate.Year} года";

        body.AppendChild(CreateParagraph(signLine, spaceAfterPt: 6));
        body.AppendChild(CreateParagraph(dateLine));
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
