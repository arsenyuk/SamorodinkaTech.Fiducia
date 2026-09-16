using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Сервис чтения данных заявок на переход доли.
/// Заменяет loopback HTTP GET-вызовы из Blazor-страниц к /api/share-requests.
/// </summary>
public interface IShareRequestReadService
{
    /// <summary>Список заявок пользователя (через EcosystemParticipant → BoardParticipant).</summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    Task<List<ShareRequest>> GetListAsync(Guid userId);

    /// <summary>Доступные типы заявок (RefRequestType).</summary>
    Task<List<RefRequestType>> GetTypesAsync();

    /// <summary>Справочные данные для форм (DocumentGroup, DocumentAccessMethod).</summary>
    Task<ShareRequestRefData> GetRefDataAsync();

    /// <summary>Детали заявки по ID.</summary>
    /// <param name="id">Идентификатор заявки.</param>
    Task<ShareRequest?> GetDetailAsync(Guid id);

    /// <summary>Поддержки коллективной заявки.</summary>
    /// <param name="requestId">Идентификатор заявки.</param>
    Task<List<ShareRequestSupport>> GetSupportsAsync(Guid requestId);

    /// <summary>Файлы заявки.</summary>
    /// <param name="requestId">Идентификатор заявки.</param>
    Task<List<ShareRequestFile>> GetFilesAsync(Guid requestId);

    /// <summary>Элементы заявки (для многоэлементных).</summary>
    /// <param name="requestId">Идентификатор заявки.</param>
    Task<List<ShareRequestItem>> GetItemsAsync(Guid requestId);

    /// <summary>Результаты заявки (для PREEMPTIVE_LIST — список участников).</summary>
    /// <param name="requestId">Идентификатор заявки.</param>
    Task<List<ShareRequestParticipantResult>> GetResultAsync(Guid requestId);

    /// <summary>Уведомления CEO по заявке (из таблицы notifications).</summary>
    /// <param name="requestId">Идентификатор заявки.</param>
    Task<List<Notification>> GetNotificationsAsync(Guid requestId);

    /// <summary>Детали CEO-требования.</summary>
    /// <param name="requestId">Идентификатор заявки.</param>
    Task<CeoDemandDetail?> GetCeoDemandAsync(Guid requestId);

    /// <summary>Список заявок для рассмотрения CEO.</summary>
    Task<List<CeoDemandListItem>> GetCeoReviewListAsync();

    /// <summary>Уведомления ВОСУ по орг-плану заявки.</summary>
    /// <param name="requestId">Идентификатор заявки.</param>
    Task<List<VosuNotification>> GetVosuNotificationsAsync(Guid requestId);

    /// <summary>Каталог предоставленных документов по ЮЛ (группировка по типам).</summary>
    /// <param name="legalEntityId">Идентификатор юридического лица.</param>
    Task<List<DocumentCatalogGroup>> GetDocumentCatalogAsync(Guid legalEntityId);
}

/// <summary>
/// Справочные данные для форм создания/редактирования заявок.
/// </summary>
public class ShareRequestRefData
{
    /// <summary>Группы документов (сгруппированы по GroupCode).</summary>
    public List<DocumentGroup> DocumentGroups { get; init; } = new();

    /// <summary>Способы доступа к документам.</summary>
    public List<DocumentAccessMethod> AccessMethods { get; init; } = new();
}

/// <summary>
/// Группа документов в справочнике (получена группировкой RefDocumentType по GroupCode).
/// </summary>
public class DocumentGroup
{
    /// <summary>Код группы (group_code).</summary>
    public string Code { get; init; } = "";

    /// <summary>Наименование группы (group_name).</summary>
    public string Name { get; init; } = "";

    /// <summary>Документы группы.</summary>
    public List<RefDocumentType> Documents { get; init; } = new();
}

/// <summary>
/// Способ доступа к документам (упрощённая модель).
/// </summary>
public class DocumentAccessMethod
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; init; }

    /// <summary>Код способа (code).</summary>
    public string Code { get; init; } = "";

    /// <summary>Наименование способа (name).</summary>
    public string Name { get; init; } = "";

    /// <summary>Описание способа (description).</summary>
    public string? Description { get; init; }

    /// <summary>Срок предоставления в рабочих днях (deadline_days).</summary>
    public int? DeadlineDays { get; init; }
}

/// <summary>
/// Результат заявки PREEMPTIVE_LIST — данные участника с долей.
/// </summary>
public class ShareRequestParticipantResult
{
    /// <summary>ФИО физлица или null для юрлица.</summary>
    public string? FullName { get; init; }

    /// <summary>Наименование юрлица-участника.</summary>
    public string? CompanyName { get; init; }

    /// <summary>ИНН юрлица-участника.</summary>
    public string? CompanyInn { get; init; }

    /// <summary>Тип участника: FL / UL / IP.</summary>
    public string? ParticipantType { get; init; }

    /// <summary>Доля участника (%).</summary>
    public decimal? SharePercent { get; init; }

    /// <summary>Номинальная стоимость доли.</summary>
    public decimal? ShareAmount { get; init; }
}

/// <summary>
/// Детали CEO-требования (для страницы рассмотрения ГД).
/// </summary>
public class CeoDemandDetail
{
    /// <summary>Заявка с навигационными свойствами.</summary>
    public ShareRequest Request { get; init; } = null!;

    /// <summary>Юридическое лицо заявки.</summary>
    public LegalEntity LegalEntity { get; init; } = null!;

    /// <summary>Участник-инициатор.</summary>
    public BoardParticipant? Participant { get; init; }

    /// <summary>Файлы заявки.</summary>
    public List<ShareRequestFile> Files { get; init; } = new();
}

/// <summary>
/// Элемент списка требований для рассмотрения CEO.
/// </summary>
public class CeoDemandListItem
{
    /// <summary>Идентификатор заявки.</summary>
    public Guid Id { get; init; }

    /// <summary>Наименование типа заявки.</summary>
    public string RequestTypeName { get; init; } = "";

    /// <summary>Наименование юридического лица.</summary>
    public string LegalEntityName { get; init; } = "";

    /// <summary>ФИО или наименование участника-инициатора.</summary>
    public string ParticipantName { get; init; } = "";

    /// <summary>Статус заявки (collective_status или status).</summary>
    public string Status { get; init; } = "";

    /// <summary>Дата создания заявки.</summary>
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Группа каталога документов (группировка по типу/заголовку).
/// </summary>
public class DocumentCatalogGroup
{
    /// <summary>Наименование группы.</summary>
    public string GroupName { get; init; } = "";

    /// <summary>Общее количество файлов.</summary>
    public int TotalFiles { get; set; }

    /// <summary>Дата последнего предоставления.</summary>
    public DateTime? LatestProvisionDate { get; set; }

    /// <summary>Требования с файлами.</summary>
    public List<DocumentCatalogDemand> Demands { get; init; } = new();
}

/// <summary>
/// Требование в каталоге документов.
/// </summary>
public class DocumentCatalogDemand
{
    /// <summary>Идентификатор заявки.</summary>
    public Guid ShareRequestId { get; init; }

    /// <summary>Номер требования.</summary>
    public string DemandNumber { get; init; } = "";

    /// <summary>Дата создания.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Статус.</summary>
    public string Status { get; init; } = "";

    /// <summary>Элементы с файлами.</summary>
    public List<DocumentCatalogItem> Items { get; init; } = new();
}

/// <summary>
/// Элемент каталога документов.
/// </summary>
public class DocumentCatalogItem
{
    /// <summary>Идентификатор элемента заявки.</summary>
    public Guid ItemId { get; init; }

    /// <summary>Заголовок.</summary>
    public string? Title { get; init; }

    /// <summary>Файлы.</summary>
    public List<DocumentCatalogFile> Files { get; init; } = new();
}

/// <summary>
/// Файл в каталоге документов.
/// </summary>
public class DocumentCatalogFile
{
    /// <summary>Идентификатор файла.</summary>
    public Guid FileId { get; init; }

    /// <summary>Имя файла.</summary>
    public string FileName { get; init; } = "";

    /// <summary>Размер в байтах.</summary>
    public long SizeBytes { get; init; }

    /// <summary>MIME-тип.</summary>
    public string? ContentType { get; init; }

    /// <summary>Дата предоставления.</summary>
    public DateTime ProvisionDate { get; init; }
}
