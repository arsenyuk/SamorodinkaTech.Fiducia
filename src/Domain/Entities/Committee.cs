using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Domain.Entities;

public class Committee
{
    /// <summary>Уникальный идентификатор (id).</summary>
    public Guid Id { get; set; }
    /// <summary>Код-аббревиатура комитета, защищённый от склонения (code).</summary>
    public string Code { get; set; } = string.Empty;
    /// <summary>Полное наименование комитета (name).</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Тип логики поведения комитета (behavior_type).</summary>
    public BehaviorType BehaviorType { get; set; }
    /// <summary>Цели и задачи комитета (description).</summary>
    public string? Description { get; set; }
    /// <summary>Признак обязательности комитета для публичных обществ / ПАО (is_mandatory_for_public).</summary>
    public bool IsMandatoryForPublic { get; set; }
    /// <summary>Признак активности комитета (is_active).</summary>
    public bool IsActive { get; set; } = true;
    /// <summary>Идентификатор председателя комитета (chair_id).</summary>
    public Guid? ChairId { get; set; }
    /// <summary>Председатель комитета.</summary>
    public User? Chair { get; set; }
    /// <summary>Идентификатор секретаря комитета (secretary_id).</summary>
    public Guid? SecretaryId { get; set; }
    /// <summary>Секретарь комитета.</summary>
    public User? Secretary { get; set; }
    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Члены комитета.</summary>
    public ICollection<CommitteeMember> Members { get; set; } = new List<CommitteeMember>();
    /// <summary>Поручения комитета.</summary>
    public ICollection<CommitteeTask> Tasks { get; set; } = new List<CommitteeTask>();
}
