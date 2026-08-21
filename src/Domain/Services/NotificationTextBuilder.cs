using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;

namespace SamorodinkaTech.Fiducia.Domain.Services;

/// <summary>
/// Формирует текст уведомлений (заголовок и тело) на основе типа уведомления.
/// Читает шаблоны из таблицы notification_template.
/// При отсутствии шаблона в БД использует дефолтные значения (fallback).
/// </summary>
public class NotificationTextBuilder
{
    private readonly IApplicationDbContext _context;

    public NotificationTextBuilder(IApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    private async Task<NotificationTemplate?> GetTemplateAsync(string typeCode, CancellationToken ct = default)
    {
        return await _context.NotificationTemplates
            .FirstOrDefaultAsync(t => t.NotificationTypeCode == typeCode && t.IsEnabled, ct);
    }

    private static string ApplyPlaceholders(string template, Dictionary<string, string> placeholders)
    {
        var result = template;
        foreach (var (key, value) in placeholders)
        {
            result = result.Replace($"{{{key}}}", value);
        }
        return result;
    }

    // ═══════════════════════════════════════════════════════════════
    // Публичные async-методы (читают шаблоны из БД)
    // ═══════════════════════════════════════════════════════════════

    public async Task<(string Title, string Body)> BuildFirstMeetingSummonsAsync(Meeting meeting, string legalEntityName, CancellationToken ct = default)
    {
        if (meeting.MeetingForm?.Code != "OCHN")
            throw new ArgumentException($"Первое заседание не может быть в форме «{meeting.MeetingForm?.Code}».", nameof(meeting));

        var template = await GetTemplateAsync("MEETING_SUMMONS", ct);
        if (template is null)
            return BuildFirstMeetingSummonsFallback(meeting, legalEntityName);

        var number = meeting.MeetingNumber ?? meeting.Id.ToString("N")[..8];
        var date = meeting.VotingStartAt ?? DateTime.UtcNow;

        var placeholders = new Dictionary<string, string>
        {
            ["legalEntityName"] = legalEntityName,
            ["meetingNumber"] = number,
            ["date"] = date.ToString("dd.MM.yyyy"),
            ["time"] = date.ToString("HH:mm"),
            ["formText"] = "очная (совместное присутствие)"
        };

        return (ApplyPlaceholders(template.TitleTemplate, placeholders),
                ApplyPlaceholders(template.BodyTemplate, placeholders));
    }

    public async Task<(string Title, string Body)> BuildMeetingSummonsAsync(Meeting meeting, CancellationToken ct = default)
    {
        var template = await GetTemplateAsync("MEETING_SUMMONS", ct);
        if (template is null)
            return BuildMeetingSummonsFallback(meeting);

        var formText = meeting.MeetingForm?.Code switch
        {
            "ZAOCHN" => "заочное",
            "MIXED" => "смешанное (очное + заочное голосование)",
            _ => "очное"
        };
        var number = meeting.MeetingNumber ?? meeting.Id.ToString("N")[..8];
        var votingLine = meeting.VotingStartAt.HasValue && meeting.VotingEndAt.HasValue
            ? $"\nГолосование: с {meeting.VotingStartAt:dd.MM.yyyy HH:mm} по {meeting.VotingEndAt:dd.MM.yyyy HH:mm} (МСК)"
            : "";

        var placeholders = new Dictionary<string, string>
        {
            ["meetingNumber"] = number,
            ["formText"] = formText,
            ["votingLine"] = votingLine
        };

        return (ApplyPlaceholders(template.TitleTemplate, placeholders),
                ApplyPlaceholders(template.BodyTemplate, placeholders));
    }

    public async Task<(string Title, string Body)> BuildVoteReminderAsync(Meeting meeting, CancellationToken ct = default)
    {
        var template = await GetTemplateAsync("VOTE_REMINDER", ct);
        if (template is null)
            return BuildVoteReminderFallback(meeting);

        var number = meeting.MeetingNumber ?? meeting.Id.ToString("N")[..8];
        var deadline = meeting.VotingEndAt.HasValue
            ? meeting.VotingEndAt.Value.ToString("dd.MM.yyyy HH:mm")
            : "установленный срок";

        var placeholders = new Dictionary<string, string>
        {
            ["meetingNumber"] = number,
            ["deadline"] = deadline
        };

        return (ApplyPlaceholders(template.TitleTemplate, placeholders),
                ApplyPlaceholders(template.BodyTemplate, placeholders));
    }

    public async Task<(string Title, string Body)> BuildVoteDeadlineAsync(Meeting meeting, CancellationToken ct = default)
    {
        var template = await GetTemplateAsync("VOTE_DEADLINE", ct);
        if (template is null)
            return BuildVoteDeadlineFallback(meeting);

        var number = meeting.MeetingNumber ?? meeting.Id.ToString("N")[..8];
        var placeholders = new Dictionary<string, string> { ["meetingNumber"] = number };

        return (ApplyPlaceholders(template.TitleTemplate, placeholders),
                ApplyPlaceholders(template.BodyTemplate, placeholders));
    }

    public async Task<(string Title, string Body)> BuildProtocolSignedAsync(Meeting meeting, CancellationToken ct = default)
    {
        var template = await GetTemplateAsync("PROTOCOL_SIGNED", ct);
        if (template is null)
            return BuildProtocolSignedFallback(meeting);

        var number = meeting.MeetingNumber ?? meeting.Id.ToString("N")[..8];
        var placeholders = new Dictionary<string, string> { ["meetingNumber"] = number };

        return (ApplyPlaceholders(template.TitleTemplate, placeholders),
                ApplyPlaceholders(template.BodyTemplate, placeholders));
    }

    public async Task<(string Title, string Body)> BuildCommitteeProtocolSignedAsync(Committee committee, string protocolNumber, CancellationToken ct = default)
    {
        var template = await GetTemplateAsync("COMMITTEE_PROTOCOL_SIGNED", ct);
        if (template is null)
            return BuildCommitteeProtocolSignedFallback(committee, protocolNumber);

        var committeeName = string.IsNullOrWhiteSpace(committee.Code)
            ? committee.Name
            : $"{committee.Code} — {committee.Name}";

        var placeholders = new Dictionary<string, string>
        {
            ["committeeName"] = committeeName,
            ["committeeDisplayName"] = committee.Name,
            ["protocolNumber"] = protocolNumber
        };

        return (ApplyPlaceholders(template.TitleTemplate, placeholders),
                ApplyPlaceholders(template.BodyTemplate, placeholders));
    }

    public async Task<(string Title, string Body)> BuildChairmanNominationAsync(string legalEntityName, CancellationToken ct = default)
    {
        var template = await GetTemplateAsync("CHAIRMAN_NOMINATION", ct);
        if (template is null)
            return BuildChairmanNominationFallback(legalEntityName);

        var placeholders = new Dictionary<string, string> { ["legalEntityName"] = legalEntityName };
        return (ApplyPlaceholders(template.TitleTemplate, placeholders),
                ApplyPlaceholders(template.BodyTemplate, placeholders));
    }

    public async Task<(string Title, string Body)> BuildDeputyChairmanNominationAsync(string legalEntityName, CancellationToken ct = default)
    {
        var template = await GetTemplateAsync("DEPUTY_CHAIRMAN_NOMINATION", ct);
        if (template is null)
            return BuildDeputyChairmanNominationFallback(legalEntityName);

        var placeholders = new Dictionary<string, string> { ["legalEntityName"] = legalEntityName };
        return (ApplyPlaceholders(template.TitleTemplate, placeholders),
                ApplyPlaceholders(template.BodyTemplate, placeholders));
    }

    public async Task<(string Title, string Body)> BuildChairmanConsentAsync(string legalEntityName, string candidateName, CancellationToken ct = default)
    {
        var template = await GetTemplateAsync("CHAIRMAN_CONSENT", ct);
        if (template is null)
            return BuildChairmanConsentFallback(legalEntityName, candidateName);

        var placeholders = new Dictionary<string, string>
        {
            ["legalEntityName"] = legalEntityName,
            ["candidateName"] = candidateName
        };
        return (ApplyPlaceholders(template.TitleTemplate, placeholders),
                ApplyPlaceholders(template.BodyTemplate, placeholders));
    }

    public async Task<(string Title, string Body)> BuildDeputyChairmanConsentAsync(string legalEntityName, string candidateName, CancellationToken ct = default)
    {
        var template = await GetTemplateAsync("DEPUTY_CHAIRMAN_CONSENT", ct);
        if (template is null)
            return BuildDeputyChairmanConsentFallback(legalEntityName, candidateName);

        var placeholders = new Dictionary<string, string>
        {
            ["legalEntityName"] = legalEntityName,
            ["candidateName"] = candidateName
        };
        return (ApplyPlaceholders(template.TitleTemplate, placeholders),
                ApplyPlaceholders(template.BodyTemplate, placeholders));
    }

    public async Task<(string Title, string Body)> BuildShareRequestAcceptedAsync(
        string creatorName, string requestType, string legalEntityName,
        string? ceoComment, string? url, CancellationToken ct = default)
    {
        var template = await GetTemplateAsync("SHARE_REQUEST_ACCEPTED", ct);
        if (template is null)
            return BuildShareRequestAcceptedFallback(creatorName, requestType, legalEntityName, ceoComment, url);

        var placeholders = new Dictionary<string, string>
        {
            ["creatorName"] = creatorName,
            ["requestType"] = requestType,
            ["legalEntityName"] = legalEntityName,
            ["ceoComment"] = ceoComment ?? "",
            ["url"] = url ?? ""
        };
        return (ApplyPlaceholders(template.TitleTemplate, placeholders),
                ApplyPlaceholders(template.BodyTemplate, placeholders));
    }

    public async Task<(string Title, string Body)> BuildShareRequestVisibleToAllAsync(
        string participantName, decimal? sharePercent, string requestType,
        string legalEntityName, string? url, CancellationToken ct = default)
    {
        var template = await GetTemplateAsync("SHARE_REQUEST_VISIBLE_TO_ALL", ct);
        if (template is null)
            return BuildShareRequestVisibleToAllFallback(participantName, sharePercent, requestType, legalEntityName, url);

        var shareText = sharePercent.HasValue ? $" (доля {sharePercent.Value:F2}%)" : "";
        var placeholders = new Dictionary<string, string>
        {
            ["participantName"] = participantName,
            ["sharePercent"] = sharePercent?.ToString("F2") ?? "",
            ["shareText"] = shareText,
            ["requestType"] = requestType,
            ["legalEntityName"] = legalEntityName,
            ["url"] = url ?? ""
        };
        return (ApplyPlaceholders(template.TitleTemplate, placeholders),
                ApplyPlaceholders(template.BodyTemplate, placeholders));
    }

    public (string Title, string Body) BuildGeneral(string title, string body) => (title, body);

    // ═══════════════════════════════════════════════════════════════
    // Приватные fallback-методы (дефолтные тексты, без БД)
    // ═══════════════════════════════════════════════════════════════

    private static (string Title, string Body) BuildFirstMeetingSummonsFallback(Meeting meeting, string legalEntityName)
    {
        var number = meeting.MeetingNumber ?? meeting.Id.ToString("N")[..8];
        var date = meeting.VotingStartAt ?? DateTime.UtcNow;
        return (
            $"Созыв первого заседания совета директоров {legalEntityName}",
            $"Уважаемый член совета директоров!\n\n"
            + $"Уведомляем вас о созыве первого (организационного) заседания совета директоров {legalEntityName}.\n\n"
            + $"Дата и время: {date:dd.MM.yyyy} в {date:HH:mm}\n"
            + $"Форма проведения: очная (совместное присутствие)\n\n"
            + $"Повестка дня:\n"
            + $"1. Избрание Председателя совета директоров.\n"
            + $"2. Избрание заместителя Председателя совета директоров.\n"
            + $"3. Избрание секретаря совета директоров.\n"
            + $"4. Формирование комитетов совета директоров.\n\n"
            + $"В соответствии с п. 1 ст. 68 Федерального закона № 208-ФЗ первое заседание совета директоров проводится только в очной форме.\n\n"
            + $"С уважением,\nСекретарь совета директоров {legalEntityName}"
        );
    }

    private static (string Title, string Body) BuildMeetingSummonsFallback(Meeting meeting)
    {
        var formText = meeting.MeetingForm?.Code switch
        {
            "ZAOCHN" => "заочное",
            "MIXED" => "смешанное (очное + заочное голосование)",
            _ => "очное"
        };
        var number = meeting.MeetingNumber ?? meeting.Id.ToString("N")[..8];
        var votingLine = meeting.VotingStartAt.HasValue && meeting.VotingEndAt.HasValue
            ? $"\nГолосование: с {meeting.VotingStartAt:dd.MM.yyyy HH:mm} по {meeting.VotingEndAt:dd.MM.yyyy HH:mm} (МСК)"
            : "";
        return (
            $"Созыв заседания совета директоров №{number}",
            $"Уведомляем вас о созыве {formText} заседания совета директоров №{number}.{votingLine}\nОзнакомьтесь с повесткой и материалами к заседанию."
        );
    }

    private static (string Title, string Body) BuildVoteReminderFallback(Meeting meeting)
    {
        var number = meeting.MeetingNumber ?? meeting.Id.ToString("N")[..8];
        var deadline = meeting.VotingEndAt.HasValue
            ? meeting.VotingEndAt.Value.ToString("dd.MM.yyyy HH:mm")
            : "установленный срок";
        return (
            $"Напоминание о голосовании — заседание №{number}",
            $"Напоминаем о необходимости проголосовать по вопросам повестки заседания №{number}. Голосование завершается {deadline} (МСК)."
        );
    }

    private static (string Title, string Body) BuildVoteDeadlineFallback(Meeting meeting)
    {
        var number = meeting.MeetingNumber ?? meeting.Id.ToString("N")[..8];
        return (
            $"Завершение голосования — заседание №{number}",
            $"Голосование по вопросам повестки заседания №{number} завершено. Результаты будут отражены в протоколе заседания."
        );
    }

    private static (string Title, string Body) BuildProtocolSignedFallback(Meeting meeting)
    {
        var number = meeting.MeetingNumber ?? meeting.Id.ToString("N")[..8];
        return (
            $"Протокол подписан — заседание №{number}",
            $"Протокол заседания совета директоров №{number} подписан. Документ доступен в разделе «Документы»."
        );
    }

    private static (string Title, string Body) BuildCommitteeProtocolSignedFallback(Committee committee, string protocolNumber)
    {
        var committeeName = string.IsNullOrWhiteSpace(committee.Code)
            ? committee.Name
            : $"{committee.Code} — {committee.Name}";
        return (
            $"Протокол подписан — {committeeName}",
            $"Протокол №{protocolNumber} комитета «{committee.Name}» подписан. Документ доступен в разделе «Документы»."
        );
    }

    private static (string Title, string Body) BuildChairmanNominationFallback(string legalEntityName)
    {
        return (
            $"Сбор предложений кандидатур — Председатель СД {legalEntityName}",
            $"Уважаемый член совета директоров!\n\n"
            + $"В рамках подготовки к первому заседанию совета директоров {legalEntityName} "
            + $"просим вас направить предложения по кандидатурам на должность Председателя совета директоров.\n\n"
            + $"Предложения принимаются до даты, установленной внутренними документами Организации.\n\n"
            + $"С уважением,\nСекретарь совета директоров {legalEntityName}"
        );
    }

    private static (string Title, string Body) BuildDeputyChairmanNominationFallback(string legalEntityName)
    {
        return (
            $"Сбор предложений кандидатур — Заместитель председателя СД {legalEntityName}",
            $"Уважаемый член совета директоров!\n\n"
            + $"В рамках подготовки к первому заседанию совета директоров {legalEntityName} "
            + $"просим вас направить предложения по кандидатурам на должность Заместителя председателя совета директоров.\n\n"
            + $"Предложения принимаются до даты, установленной внутренними документами Организации.\n\n"
            + $"С уважением,\nСекретарь совета директоров {legalEntityName}"
        );
    }

    private static (string Title, string Body) BuildChairmanConsentFallback(string legalEntityName, string candidateName)
    {
        return (
            $"Согласие на должность Председателя СД {legalEntityName}",
            $"Уважаемый {candidateName}!\n\n"
            + $"Ваша кандидатура предложена на должность Председателя совета директоров {legalEntityName}. "
            + $"Для включения в бюллетень голосования необходимо подписать согласие на выдвижение.\n\n"
            + $"Подписание согласия равнозначно собственноручной подписи в соответствии с Федеральным законом № 63-ФЗ.\n\n"
            + $"С уважением,\nСекретарь совета директоров {legalEntityName}"
        );
    }

    private static (string Title, string Body) BuildDeputyChairmanConsentFallback(string legalEntityName, string candidateName)
    {
        return (
            $"Согласие на должность Заместителя председателя СД {legalEntityName}",
            $"Уважаемый {candidateName}!\n\n"
            + $"Ваша кандидатура предложена на должность Заместителя председателя совета директоров {legalEntityName}. "
            + $"Для включения в бюллетень голосования необходимо подписать согласие на выдвижение.\n\n"
            + $"Подписание согласия равнозначно собственноручной подписи в соответствии с Федеральным законом № 63-ФЗ.\n\n"
            + $"С уважением,\nСекретарь совета директоров {legalEntityName}"
        );
    }

    private static (string Title, string Body) BuildShareRequestAcceptedFallback(
        string creatorName, string requestType, string legalEntityName, string? ceoComment, string? url)
    {
        var commentLine = string.IsNullOrWhiteSpace(ceoComment) ? "" : $"\n\nКомментарий ГД: {ceoComment}";
        var urlLine = string.IsNullOrWhiteSpace(url) ? "" : $"\n\nПодробности: {url}";
        return (
            $"ИМИТАЦИЯ ОТПРАВКА ПО email — Требование принято",
            $"Уважаемый(-ая) {creatorName}!\n\n"
            + $"Ваше требование (тип: {requestType}) по обществу «{legalEntityName}» принято Генеральным директором."
            + commentLine + urlLine
        );
    }

    private static (string Title, string Body) BuildShareRequestVisibleToAllFallback(
        string participantName, decimal? sharePercent, string requestType, string legalEntityName, string? url)
    {
        var shareText = sharePercent.HasValue ? $" (доля {sharePercent.Value:F2}%)" : "";
        var urlLine = string.IsNullOrWhiteSpace(url) ? "" : $"\n\nОзнакомьтесь с требованием: {url}";
        return (
            $"ИМИТАЦИЯ ОТПРАВКА ПО email — Новое требование участника",
            $"Уважаемый Генеральный директор!\n\n"
            + $"Поступило требование участника {participantName}{shareText} по обществу «{legalEntityName}».\n\n"
            + $"Тип требования: {requestType}" + urlLine
        );
    }
}
