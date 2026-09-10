using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Генератор DOCX-уведомления о созыве/изменении повестки ВОСУ в формате ГОСТ.
/// Times New Roman 14pt, поля: левое 30мм, правое 15мм, верх/низ 20мм, интервал 1.5.
/// </summary>
public class VosuNotificationDocxGenerator : IVosuNotificationDocxGenerator
{
    private const string FontName = "Times New Roman";
    private const int FontSizePt = 14;
    private const int LeftMarginMm = 30;
    private const int RightMarginMm = 15;
    private const int TopMarginMm = 20;
    private const int BottomMarginMm = 20;
    private const int LineSpacing15 = 360;

    /// <summary>Точка в миллиметрах (1 инч = 25.4 мм, 1 pt = 1/72 инча).</summary>
    private static int MmToTwips(int mm) => (int)(mm * 56.692913);

    /// <summary>Размер шрифта в half-points (14pt = 28 half-points).</summary>
    private static int PtToHalfPt(int pt) => pt * 2;

    public Task<byte[]> GenerateAsync(VosuNotificationData data, CancellationToken ct = default)
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

            if (data.IsAgendaChange)
                AddAgendaChangeNotification(body, data);
            else
                AddInitialNotification(body, data);
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
        bool right = false, int? spaceAfterPt = null)
    {
        var para = new Paragraph();
        var pPr = new ParagraphProperties();

        if (center)
            pPr.AppendChild(new Justification { Val = JustificationValues.Center });
        else if (right)
            pPr.AppendChild(new Justification { Val = JustificationValues.Right });

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

    private static Paragraph CreateAddressBlock(string line1, string? line2)
    {
        var para = new Paragraph();
        var pPr = new ParagraphProperties();
        pPr.AppendChild(new Justification { Val = JustificationValues.Left });
        para.AppendChild(pPr);

        var run = new Run();
        run.AppendChild(new RunProperties(
            new RunFonts { Ascii = FontName, HighAnsi = FontName, ComplexScript = FontName },
            new FontSize { Val = PtToHalfPt(FontSizePt).ToString() }));
        run.AppendChild(new Text(line1) { Space = SpaceProcessingModeValues.Preserve });
        para.AppendChild(run);

        if (!string.IsNullOrWhiteSpace(line2))
        {
            para.AppendChild(new Run(new Break()));
            var run2 = new Run();
            run2.AppendChild(new RunProperties(
                new RunFonts { Ascii = FontName, HighAnsi = FontName, ComplexScript = FontName },
                new FontSize { Val = PtToHalfPt(FontSizePt).ToString() }));
            run2.AppendChild(new Text(line2) { Space = SpaceProcessingModeValues.Preserve });
            para.AppendChild(run2);
        }

        return para;
    }

    private static void AddAgendaChangeNotification(Body body, VosuNotificationData data)
    {
        var dateStr = data.NotificationDate.ToString("«dd» MMMM yyyy") + " г.";
        var leInfo = BuildLegalEntityInfo(data);

        body.AppendChild(CreateParagraph(leInfo, spaceAfterPt: 12));
        body.AppendChild(CreateParagraph("УВЕДОМЛЕНИЕ", bold: true, center: true, spaceAfterPt: 6));
        body.AppendChild(CreateParagraph("об изменении повестки дня внеочередного общего собрания участников",
            bold: true, center: true, spaceAfterPt: 12));

        var introText = BuildIntroText(data);
        body.AppendChild(CreateParagraph(introText, spaceAfterPt: 12));

        AddMeetingDetails(body, data);
        AddAgendaSection(body, data);
        AddReviewSection(body, data);
        AddSignature(body, data);
    }

    private static void AddInitialNotification(Body body, VosuNotificationData data)
    {
        var leInfo = BuildLegalEntityInfo(data);

        body.AppendChild(CreateParagraph(leInfo, spaceAfterPt: 12));
        body.AppendChild(CreateParagraph("УВЕДОМЛЕНИЕ", bold: true, center: true, spaceAfterPt: 6));
        body.AppendChild(CreateParagraph("о созыве внеочередного общего собрания участников",
            bold: true, center: true, spaceAfterPt: 12));

        var introText = BuildInitialIntroText(data);
        body.AppendChild(CreateParagraph(introText, spaceAfterPt: 12));

        AddMeetingDetails(body, data);
        AddAgendaSection(body, data);
        AddReviewSection(body, data);
        AddSignature(body, data);
    }

    private static string BuildLegalEntityInfo(VosuNotificationData data)
    {
        var parts = new List<string> { data.LegalEntityName };
        if (!string.IsNullOrWhiteSpace(data.LegalEntityOgrn))
            parts.Add($"ОГРН {data.LegalEntityOgrn}");
        if (!string.IsNullOrWhiteSpace(data.LegalEntityInn))
            parts.Add($"ИНН {data.LegalEntityInn}");
        return string.Join(", ", parts);
    }

    private static string BuildIntroText(VosuNotificationData data)
    {
        var demandDate = data.DemandReceivedDate.HasValue
            ? $"«{data.DemandReceivedDate.Value.Day}» {FormatMonth(data.DemandReceivedDate.Value.Month)} {data.DemandReceivedDate.Value.Year} года"
            : "";

        var initiatorLine = !string.IsNullOrWhiteSpace(data.InitiatorName)
            ? $"участника Общества ({data.InitiatorName}" +
              (data.InitiatorSharePercent.HasValue ? $", владеющего {data.InitiatorSharePercent.Value}% уставного капитала" : "") +
              ")"
            : "участника Общества";

        return $"Настоящим {data.LegalEntityName} (ОГРН {data.LegalEntityOgrn ?? "___"}, ИНН {data.LegalEntityInn ?? "___"}) " +
               $"извещает Вас о том, что на основании заявления {initiatorLine}, " +
               $"поступившего в адрес Общества {demandDate} " +
               $"в соответствии с пунктом 2 статьи 36 Федерального закона № 14-ФЗ «Об обществах с ограниченной ответственностью», " +
               $"в первоначальную повестку дня внеочередного общего собрания участников внесены изменения.";
    }

    private static string BuildInitialIntroText(VosuNotificationData data)
    {
        return $"Настоящим {data.LegalEntityName} (ОГРН {data.LegalEntityOgrn ?? "___"}, ИНН {data.LegalEntityInn ?? "___"}) " +
               $"извещает Вас о созыве внеочередного общего собрания участников.";
    }

    private static void AddMeetingDetails(Body body, VosuNotificationData data)
    {
        body.AppendChild(CreateParagraph("Внеочередное общее собрание участников состоится:", bold: true, spaceAfterPt: 6));

        var dateStr = data.MeetingDate.ToString("dd.MM.yyyy");
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

    private static void AddAgendaSection(Body body, VosuNotificationData data)
    {
        body.AppendChild(new Paragraph());
        var agendaTitle = data.IsAgendaChange
            ? "С учетом поступивших требований утверждена следующая ОКОНЧАТЕЛЬНАЯ ПОВЕСТКА ДНЯ ВОСУ:"
            : "ПОВЕСТКА ДНЯ ВОСУ:";
        body.AppendChild(CreateParagraph(agendaTitle, bold: true, spaceAfterPt: 6));

        for (int i = 0; i < data.AgendaItems.Count; i++)
        {
            var itemText = $"{i + 1}. {data.AgendaItems[i]}";
            body.AppendChild(CreateParagraph(itemText, spaceAfterPt: 3));
        }
    }

    private static void AddReviewSection(Body body, VosuNotificationData data)
    {
        body.AppendChild(new Paragraph());
        var reviewText = string.IsNullOrWhiteSpace(data.ReviewLocation)
            ? "Ознакомиться с материалами и документами можно в рабочие дни с 10:00 до 16:00."
            : $"Ознакомиться с обновлённым пакетом материалов и документов можно по адресу: {data.ReviewLocation}.";
        body.AppendChild(CreateParagraph(reviewText, spaceAfterPt: 18));
    }

    private static void AddSignature(Body body, VosuNotificationData data)
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
