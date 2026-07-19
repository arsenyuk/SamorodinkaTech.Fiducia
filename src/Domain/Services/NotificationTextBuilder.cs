using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Domain.Services;

/// <summary>
/// Формирует текст уведомлений (заголовок и тело) на основе типа уведомления
/// и связанных сущностей.
/// </summary>
public class NotificationTextBuilder
{
    /// <summary>
    /// Созыв первого (организационного) заседания совета директоров.
    /// Первое заседание не может быть заочным: избрание Председателя СД,
    /// заместителя, секретаря и формирование комитетов требуют
    /// совместного обсуждения (ст. 68 208-ФЗ, п. 1).
    /// </summary>
    /// <param name="meeting">Заседание (MeetingForm игнорируется — всегда очное).</param>
    /// <param name="legalEntityName">Полное наименование юридического лица.</param>
    public (string Title, string Body) BuildFirstMeetingSummons(Meeting meeting, string legalEntityName)
    {
        if (meeting.MeetingForm?.Code != "OCHN")
        {
            throw new ArgumentException(
                $"Первое заседание совета директоров не может быть проведено " +
                $"в форме «{meeting.MeetingForm?.Code ?? "(не указана)"}». " +
                $"Согласно п. 1 ст. 68 208-ФЗ первое (организационное) заседание " +
                $"проводится только в очной форме (OCHN).",
                nameof(meeting));
        }

        var number = meeting.MeetingNumber ?? meeting.Id.ToString("N")[..8];
        var date = meeting.VotingStartAt ?? DateTime.UtcNow;

        return (
            $"Созыв первого заседания совета директоров {legalEntityName}",
            $"Уважаемый член совета директоров!\n\n"
            + $"Уведомляем вас о созыве первого (организационного) заседания "
            + $"совета директоров {legalEntityName}.\n\n"
            + $"Дата и время: {date:dd.MM.yyyy} в {date:HH:mm}\n"
            + $"Форма проведения: очная (совместное присутствие)\n\n"
            + $"Повестка дня:\n"
            + $"1. Избрание Председателя совета директоров.\n"
            + $"2. Избрание заместителя Председателя совета директоров.\n"
            + $"3. Избрание секретаря совета директоров.\n"
            + $"4. Формирование комитетов совета директоров.\n\n"
            + $"В соответствии с п. 1 ст. 68 Федерального закона № 208-ФЗ "
            + $"«Об акционерных обществах» первое заседание совета директоров "
            + $"проводится только в очной форме. Заочная форма для первого "
            + $"заседания не допускается: избрание Председателя СД требует "
            + $"совместного обсуждения и присутствия членов совета директоров.\n\n"
            + $"Материалы по вопросам повестки дня приложены к настоящему уведомлению.\n\n"
            + $"С уважением,\n"
            + $"Секретарь совета директоров {legalEntityName}"
        );
    }

    /// <summary>
    /// Созыв заседания совета директоров.
    /// </summary>
    public (string Title, string Body) BuildMeetingSummons(Meeting meeting)
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

    /// <summary>
    /// Напоминание о необходимости проголосовать.
    /// </summary>
    public (string Title, string Body) BuildVoteReminder(Meeting meeting)
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

    /// <summary>
    /// Завершение голосования.
    /// </summary>
    public (string Title, string Body) BuildVoteDeadline(Meeting meeting)
    {
        var number = meeting.MeetingNumber ?? meeting.Id.ToString("N")[..8];

        return (
            $"Завершение голосования — заседание №{number}",
            $"Голосование по вопросам повестки заседания №{number} завершено. Результаты будут отражены в протоколе заседания."
        );
    }

    /// <summary>
    /// Протокол заседания совета директоров подписан.
    /// </summary>
    public (string Title, string Body) BuildProtocolSigned(Meeting meeting)
    {
        var number = meeting.MeetingNumber ?? meeting.Id.ToString("N")[..8];

        return (
            $"Протокол подписан — заседание №{number}",
            $"Протокол заседания совета директоров №{number} подписан. Документ доступен в разделе «Документы»."
        );
    }

    /// <summary>
    /// Протокол заседания комитета подписан.
    /// </summary>
    public (string Title, string Body) BuildCommitteeProtocolSigned(Committee committee, string protocolNumber)
    {
        var committeeName = string.IsNullOrWhiteSpace(committee.Code)
            ? committee.Name
            : $"{committee.Code} — {committee.Name}";

        return (
            $"Протокол подписан — {committeeName}",
            $"Протокол №{protocolNumber} комитета «{committee.Name}» подписан. Документ доступен в разделе «Документы»."
        );
    }

    /// <summary>
    /// Общее уведомление — текст задаётся вызывающей стороной.
    /// </summary>
    public (string Title, string Body) BuildGeneral(string title, string body)
    {
        return (title, body);
    }
}
