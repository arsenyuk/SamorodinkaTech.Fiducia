namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Физическое лицо (persons).
/// Хранит данные ФИО, ИНН, контактные данные физического лица.
/// </summary>
public class Person
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Фамилия (last_name).</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Имя (first_name).</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Отчество (middle_name).</summary>
    public string? MiddleName { get; set; }

    /// <summary>Email (email).</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Телефон (phone).</summary>
    public string? Phone { get; set; }

    /// <summary>ИНН (inn).</summary>
    public string? Inn { get; set; }

    /// <summary>Дата создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Кто создал запись (created_by).</summary>
    public Guid CreatedBy { get; set; }

    public User? CreatedByUser { get; set; }
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<PdnConsent> PdnConsents { get; set; } = new List<PdnConsent>();
    public ICollection<PepAgreement> PepAgreements { get; set; } = new List<PepAgreement>();
    public ICollection<IndependenceDeclaration> IndependenceDeclarations { get; set; } = new List<IndependenceDeclaration>();
}