using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Сервис управления договорами: создание и обновление без loopback HTTP.
/// Заменяет POST/PUT эндпоинты ContractEndpoints.
/// </summary>
public interface IContractService
{
    /// <summary>
    /// Создаёт новый договор.
    /// </summary>
    /// <param name="model">Данные договора.</param>
    /// <param name="fileStream">Поток файла договора (опционально).</param>
    /// <param name="fileName">Оригинальное имя файла договора.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Идентификатор созданного договора.</returns>
    Task<Guid> CreateAsync(ContractCreateModel model, Stream? fileStream, string? fileName, CancellationToken ct = default);

    /// <summary>
    /// Обновляет существующий договор.
    /// </summary>
    /// <param name="id">Идентификатор договора.</param>
    /// <param name="model">Обновлённые данные договора.</param>
    /// <param name="fileStream">Поток нового файла договора (опционально).</param>
    /// <param name="fileName">Оригинальное имя файла.</param>
    /// <param name="ct">Токен отмены.</param>
    Task UpdateAsync(Guid id, ContractUpdateModel model, Stream? fileStream, string? fileName, CancellationToken ct = default);
}

/// <summary>
/// Модель для создания договора.
/// </summary>
public class ContractCreateModel
{
    /// <summary>Идентификатор юридического лица.</summary>
    public Guid LegalEntityId { get; init; }

    /// <summary>Идентификатор текущего пользователя.</summary>
    public Guid UserId { get; init; }

    /// <summary>Тип договора.</summary>
    public ContractType ContractType { get; init; }

    /// <summary>Наименование контрагента.</summary>
    public string CounterpartyName { get; init; } = default!;

    /// <summary>ИНН контрагента.</summary>
    public string CounterpartyInn { get; init; } = default!;

    /// <summary>Номер договора.</summary>
    public string? ContractNumber { get; init; }

    /// <summary>Дата договора.</summary>
    public DateOnly? ContractDate { get; init; }

    /// <summary>Начало действия договора.</summary>
    public DateOnly? ContractValidFrom { get; init; }

    /// <summary>Окончание действия договора.</summary>
    public DateOnly? ContractValidTo { get; init; }

    /// <summary>Признак бессрочного договора.</summary>
    public bool IsIndefinite { get; init; } = true;

    /// <summary>ОГРНИП управляющего ИП (MANAGEMENT_IP).</summary>
    public string? ManagerOgrnip { get; init; }

    /// <summary>Идентификатор ЮЛ-управляющего (MANAGEMENT_UL).</summary>
    public Guid? ManagerLegalEntityId { get; init; }

    /// <summary>Срок подготовки реестра — количество (REGISTRAR).</summary>
    public int? RegistryPreparationDays { get; init; }

    /// <summary>Код единицы измерения срока подготовки реестра (REGISTRAR).</summary>
    public string? RegistryPreparationUnit { get; init; }

    /// <summary>Срок подготовки дивидендного реестра — количество (REGISTRAR).</summary>
    public int? DividendRegistryPreparationDays { get; init; }

    /// <summary>Код единицы измерения дивидендного реестра (REGISTRAR).</summary>
    public string? DividendRegistryPreparationUnit { get; init; }

    /// <summary>URL правил ведения реестра (REGISTRAR).</summary>
    public string? RegistryRulesUrl { get; init; }

    /// <summary>Поток файла правил реестра (REGISTRAR).</summary>
    public Stream? RulesFileStream { get; init; }

    /// <summary>Имя файла правил реестра (REGISTRAR).</summary>
    public string? RulesFileName { get; init; }
}

/// <summary>
/// Модель для обновления договора.
/// </summary>
public class ContractUpdateModel
{
    /// <summary>Номер договора.</summary>
    public string? ContractNumber { get; init; }

    /// <summary>Дата договора.</summary>
    public DateOnly? ContractDate { get; init; }

    /// <summary>Начало действия договора.</summary>
    public DateOnly? ContractValidFrom { get; init; }

    /// <summary>Окончание действия договора.</summary>
    public DateOnly? ContractValidTo { get; init; }

    /// <summary>Признак бессрочного договора.</summary>
    public bool IsIndefinite { get; init; } = true;

    /// <summary>ОГРНИП управляющего ИП (MANAGEMENT_IP).</summary>
    public string? ManagerOgrnip { get; init; }

    /// <summary>Идентификатор ЮЛ-управляющего (MANAGEMENT_UL).</summary>
    public Guid? ManagerLegalEntityId { get; init; }

    /// <summary>Срок подготовки реестра — количество (REGISTRAR).</summary>
    public int? RegistryPreparationDays { get; init; }

    /// <summary>Код единицы измерения срока подготовки реестра (REGISTRAR).</summary>
    public string? RegistryPreparationUnit { get; init; }

    /// <summary>Срок подготовки дивидендного реестра — количество (REGISTRAR).</summary>
    public int? DividendRegistryPreparationDays { get; init; }

    /// <summary>Код единицы измерения дивидендного реестра (REGISTRAR).</summary>
    public string? DividendRegistryPreparationUnit { get; init; }

    /// <summary>URL правил ведения реестра (REGISTRAR).</summary>
    public string? RegistryRulesUrl { get; init; }

    /// <summary>Поток файла правил реестра (REGISTRAR).</summary>
    public Stream? RulesFileStream { get; init; }

    /// <summary>Имя файла правил реестра (REGISTRAR).</summary>
    public string? RulesFileName { get; init; }
}
