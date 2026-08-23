using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Domain.Models;

/// <summary>
/// Настройки типового устава ООО для отображения в UI.
/// Формируется из сущности БД <see cref="RefStandardCharter"/> — без хардкода данных.
/// Номера 01–09 соответствуют формату ФНС для форм Р11001 и Р13014.
/// </summary>
public record StandardCharterInfo(
    bool ExitAllowed,
    bool TransferToParticipantsWithoutConsent,
    bool TransferToThirdPartiesWithoutConsent,
    bool PreemptiveRight,
    bool InheritanceWithoutConsent,
    char ExecutiveBody,
    Guid ProtocolConfirmationMethodId,
    string ProtocolConfirmationLabel
)
{
    /// <summary>Создаёт модель отображения из сущности БД.</summary>
    public static StandardCharterInfo FromEntity(RefStandardCharter entity) => new(
        entity.ExitAllowed,
        entity.TransferToParticipantsWithoutConsent,
        entity.TransferToThirdPartiesWithoutConsent,
        entity.PreemptiveRight,
        entity.InheritanceWithoutConsent,
        entity.ExecutiveBody,
        entity.ProtocolConfirmationMethodId,
        entity.ProtocolConfirmationMethod?.Name ?? "Нотариальное удостоверение"
    );

    public string ExitLabel => ExitAllowed
        ? "Выход участника разрешён (независимо от согласия остальных)"
        : "Выход участника не предусмотрен";

    public string TransferToParticipantsLabel => TransferToParticipantsWithoutConsent
        ? "Без согласия остальных участников"
        : "С согласия остальных участников";

    public string TransferToThirdPartiesLabel => TransferToThirdPartiesWithoutConsent
        ? "Без согласия остальных участников"
        : "С согласия остальных участников";

    public string PreemptiveRightLabel => PreemptiveRight
        ? "Участники пользуются преимущественным правом покупки"
        : "Участники не обладают преимущественным правом";

    public string InheritanceLabel => InheritanceWithoutConsent
        ? "Без согласия остальных участников"
        : "С согласия остальных участников";

    public string ExecutiveBodyLabel => ExecutiveBody switch
    {
        'A' => "Одно лицо (генеральный директор), избирается на 5 лет",
        'B' => "Каждый участник — самостоятельно действующий директор",
        'C' => "Все участники общества — совместно действующие директора",
        _ => "—"
    };
}
