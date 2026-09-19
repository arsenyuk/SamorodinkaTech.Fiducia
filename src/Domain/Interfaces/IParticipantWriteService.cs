namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Сервис записи данных участников общества (Board Portal).
/// Заменяет POST/PUT/DELETE loopback HTTP-вызовы из Blazor-страниц.
/// </summary>
public interface IParticipantWriteService
{
    /// <summary>Создать участника.</summary>
    Task<Guid> CreateAsync(ParticipantCreateModel model, Guid userId, string clientIp, CancellationToken ct = default);

    /// <summary>Обновить участника.</summary>
    Task UpdateAsync(Guid id, ParticipantUpdateModel model, Guid userId, string clientIp, CancellationToken ct = default);

    /// <summary>Удалить участника (жёсткое удаление + деактивация CEO).</summary>
    Task DeleteAsync(Guid id, Guid userId, string clientIp, CancellationToken ct = default);

    /// <summary>Импорт участников из СПАРК.</summary>
    Task ImportFromSparkAsync(Guid legalEntityId, Guid userId, string clientIp, CancellationToken ct = default);

    /// <summary>Создать казначейскую долю.</summary>
    Task<Guid> CreateTreasuryAsync(TreasuryCreateModel model, Guid userId, string clientIp, CancellationToken ct = default);

    /// <summary>Обновить казначейскую долю.</summary>
    Task UpdateTreasuryAsync(Guid id, TreasuryUpdateModel model, Guid userId, string clientIp, CancellationToken ct = default);

    /// <summary>Удалить казначейскую долю.</summary>
    Task DeleteTreasuryAsync(Guid id, Guid userId, string clientIp, CancellationToken ct = default);

    /// <summary>Удалить загрузку реестра.</summary>
    Task DeleteRegistryUploadAsync(Guid id, Guid userId, string clientIp, CancellationToken ct = default);

    /// <summary>Создать запись информирования об изменении сведений.</summary>
    Task<Guid> CreateChangeAsync(ChangeCreateModel model, Guid userId, string clientIp, CancellationToken ct = default);

    /// <summary>Загрузить документ-подтверждение к информированию.</summary>
    Task UploadChangeDocumentAsync(Guid changeId, Stream fileStream, string fileName, string contentType, Guid userId, string clientIp, CancellationToken ct = default);
}

/// <summary>Модель создания участника.</summary>
public record ParticipantCreateModel
{
    /// <summary>Идентификатор юридического лица.</summary>
    public Guid LegalEntityId { get; init; }

    /// <summary>Тип участника (FL / UL / IP).</summary>
    public string? ParticipantType { get; init; }

    /// <summary>Идентификатор участника экосистемы (привязка).</summary>
    public Guid? EcosystemParticipantId { get; init; }

    /// <summary>Фамилия (для ФЛ).</summary>
    public string? LastName { get; init; }

    /// <summary>Имя (для ФЛ).</summary>
    public string? FirstName { get; init; }

    /// <summary>Отчество (для ФЛ).</summary>
    public string? MiddleName { get; init; }

    /// <summary>Код типа ДУЛ.</summary>
    public string? DulTypeCode { get; init; }

    /// <summary>Серия ДУЛ.</summary>
    public string? DulSeries { get; init; }

    /// <summary>Номер ДУЛ.</summary>
    public string? DulNumber { get; init; }

    /// <summary>Кем выдан паспорт.</summary>
    public string? PassportIssuedBy { get; init; }

    /// <summary>Дата выдачи паспорта.</summary>
    public DateOnly? PassportIssueDate { get; init; }

    /// <summary>Код подразделения.</summary>
    public string? PassportDepartmentCode { get; init; }

    /// <summary>Адрес регистрации.</summary>
    public string? PassportRegistrationAddress { get; init; }

    /// <summary>ИНН физического лица.</summary>
    public string? PersonInn { get; init; }

    /// <summary>Гражданство.</summary>
    public string? Citizenship { get; init; }

    /// <summary>Адрес электронной почты.</summary>
    public string? Email { get; init; }

    /// <summary>Наименование ЮЛ (для UL).</summary>
    public string? CompanyName { get; init; }

    /// <summary>ИНН ЮЛ (для UL).</summary>
    public string? CompanyInn { get; init; }

    /// <summary>ОГРН ЮЛ (для UL).</summary>
    public string? CompanyOgrn { get; init; }

    /// <summary>КПП ЮЛ (для UL).</summary>
    public string? CompanyKpp { get; init; }

    /// <summary>Адрес ЮЛ (для UL).</summary>
    public string? CompanyAddress { get; init; }

    /// <summary>ОГРНИП (для ИП).</summary>
    public string? Ogrnip { get; init; }

    /// <summary>Размер доли (%).</summary>
    public decimal? SharePercent { get; init; }

    /// <summary>Доля в виде простой дроби (share_fraction).</summary>
    public string? ShareFraction { get; init; }

    /// <summary>Стоимость доли (руб.).</summary>
    public decimal? ShareAmount { get; init; }

    /// <summary>Сведения об оплате.</summary>
    public string? PaymentInfo { get; init; }

    /// <summary>Сведения о регистрации доли.</summary>
    public string? ShareRegistrationInfo { get; init; }

    /// <summary>Дата вступления.</summary>
    public DateOnly? EntryDate { get; init; }

    /// <summary>Дата выхода.</summary>
    public DateOnly? ExitDate { get; init; }

    /// <summary>Признак активности.</summary>
    public bool? IsActive { get; init; }
}

/// <summary>Модель обновления участника.</summary>
public record ParticipantUpdateModel
{
    /// <summary>Тип участника (FL / UL / IP).</summary>
    public string? ParticipantType { get; init; }

    /// <summary>Фамилия (для ФЛ).</summary>
    public string? LastName { get; init; }

    /// <summary>Имя (для ФЛ).</summary>
    public string? FirstName { get; init; }

    /// <summary>Отчество (для ФЛ).</summary>
    public string? MiddleName { get; init; }

    /// <summary>Адрес электронной почты.</summary>
    public string? Email { get; init; }

    /// <summary>Наименование ЮЛ (для UL).</summary>
    public string? CompanyName { get; init; }

    /// <summary>ИНН ЮЛ (для UL).</summary>
    public string? CompanyInn { get; init; }

    /// <summary>ОГРН ЮЛ (для UL).</summary>
    public string? CompanyOgrn { get; init; }

    /// <summary>КПП ЮЛ (для UL).</summary>
    public string? CompanyKpp { get; init; }

    /// <summary>Адрес ЮЛ (для UL).</summary>
    public string? CompanyAddress { get; init; }

    /// <summary>Размер доли (%).</summary>
    public decimal? SharePercent { get; init; }

    /// <summary>Доля в виде простой дроби (share_fraction).</summary>
    public string? ShareFraction { get; init; }

    /// <summary>Стоимость доли (руб.).</summary>
    public decimal? ShareAmount { get; init; }

    /// <summary>Сведения об оплате.</summary>
    public string? PaymentInfo { get; init; }

    /// <summary>Сведения о регистрации доли.</summary>
    public string? ShareRegistrationInfo { get; init; }

    /// <summary>Дата вступления.</summary>
    public DateOnly? EntryDate { get; init; }

    /// <summary>Дата выхода.</summary>
    public DateOnly? ExitDate { get; init; }

    /// <summary>Признак активности.</summary>
    public bool? IsActive { get; init; }
}

/// <summary>Модель создания казначейской доли.</summary>
public record TreasuryCreateModel
{
    /// <summary>Идентификатор юридического лица.</summary>
    public Guid LegalEntityId { get; init; }

    /// <summary>Размер доли (%).</summary>
    public decimal? SharePercent { get; init; }

    /// <summary>Доля в виде простой дроби (share_fraction).</summary>
    public string? ShareFraction { get; init; }

    /// <summary>Стоимость доли (руб.).</summary>
    public decimal? ShareAmount { get; init; }

    /// <summary>Дата приобретения.</summary>
    public DateOnly? AcquiredDate { get; init; }

    /// <summary>Основание приобретения.</summary>
    public string? AcquisitionBasis { get; init; }
}

/// <summary>Модель обновления казначейской доли.</summary>
public record TreasuryUpdateModel
{
    /// <summary>Размер доли (%).</summary>
    public decimal? SharePercent { get; init; }

    /// <summary>Доля в виде простой дроби (share_fraction).</summary>
    public string? ShareFraction { get; init; }

    /// <summary>Стоимость доли (руб.).</summary>
    public decimal? ShareAmount { get; init; }

    /// <summary>Дата приобретения.</summary>
    public DateOnly? AcquiredDate { get; init; }

    /// <summary>Основание приобретения.</summary>
    public string? AcquisitionBasis { get; init; }
}

/// <summary>Модель создания записи информирования об изменении сведений.</summary>
public record ChangeCreateModel
{
    /// <summary>Идентификатор юридического лица.</summary>
    public Guid LegalEntityId { get; init; }

    /// <summary>Идентификатор участника.</summary>
    public Guid ParticipantId { get; init; }

    /// <summary>Тип участника (FL / UL / IP).</summary>
    public string? ParticipantType { get; init; }

    /// <summary>Фамилия.</summary>
    public string? LastName { get; init; }

    /// <summary>Имя.</summary>
    public string? FirstName { get; init; }

    /// <summary>Отчество.</summary>
    public string? MiddleName { get; init; }

    /// <summary>Тип ДУЛ.</summary>
    public Guid? DulTypeId { get; init; }

    /// <summary>Серия паспорта.</summary>
    public string? PassportSeries { get; init; }

    /// <summary>Номер паспорта.</summary>
    public string? PassportNumber { get; init; }

    /// <summary>Кем выдан паспорт.</summary>
    public string? PassportIssuedBy { get; init; }

    /// <summary>Дата выдачи паспорта.</summary>
    public DateOnly? PassportIssueDate { get; init; }

    /// <summary>Код подразделения.</summary>
    public string? PassportDepartmentCode { get; init; }

    /// <summary>Адрес регистрации.</summary>
    public string? PassportRegistrationAddress { get; init; }

    /// <summary>ИНН физического лица.</summary>
    public string? PersonInn { get; init; }

    /// <summary>Гражданство.</summary>
    public string? Citizenship { get; init; }

    /// <summary>Адрес электронной почты.</summary>
    public string? Email { get; init; }

    /// <summary>Наименование ЮЛ (для UL).</summary>
    public string? CompanyName { get; init; }

    /// <summary>ИНН ЮЛ (для UL).</summary>
    public string? CompanyInn { get; init; }

    /// <summary>ОГРН ЮЛ (для UL).</summary>
    public string? CompanyOgrn { get; init; }

    /// <summary>КПП ЮЛ (для UL).</summary>
    public string? CompanyKpp { get; init; }

    /// <summary>Адрес ЮЛ (для UL).</summary>
    public string? CompanyAddress { get; init; }

    /// <summary>ОГРНИП.</summary>
    public string? Ogrnip { get; init; }

    /// <summary>Размер доли (%).</summary>
    public decimal? SharePercent { get; init; }

    /// <summary>Доля в виде простой дроби (share_fraction).</summary>
    public string? ShareFraction { get; init; }

    /// <summary>Стоимость доли (руб.).</summary>
    public decimal? ShareAmount { get; init; }

    /// <summary>Идентификатор файла документа.</summary>
    public Guid? DocumentFileId { get; init; }

    /// <summary>Оригинальное имя файла документа.</summary>
    public string? DocumentOriginalName { get; init; }

    /// <summary>Источник (electronic / paper).</summary>
    public string? Source { get; init; }

    /// <summary>Дата документа.</summary>
    public string? Date { get; init; }

    /// <summary>Номер бумажного документа.</summary>
    public string? PaperDocNumber { get; init; }

    /// <summary>Комментарий.</summary>
    public string? Comment { get; init; }
}
