namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Доля, принадлежащая Обществу — казначейская доля (board_treasury_share).
/// </summary>
public class BoardTreasuryShare
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор юридического лица (legal_entity_id).</summary>
    public Guid LegalEntityId { get; set; }

    /// <summary>Размер доли в процентах (share_percent).</summary>
    public decimal? SharePercent { get; set; }

    /// <summary>Номинальная стоимость доли в рублях (share_amount).</summary>
    public decimal? ShareAmount { get; set; }

    /// <summary>Дата перехода/приобретения доли Обществом (acquired_date).</summary>
    public DateOnly? AcquiredDate { get; set; }

    /// <summary>Основание перехода доли (acquisition_basis).</summary>
    public string? AcquisitionBasis { get; set; }

    /// <summary>Порядок сортировки (sort_order).</summary>
    public int SortOrder { get; set; }

    /// <summary>Время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Время последнего обновления (updated_at).</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Идентификатор создателя записи (created_by).</summary>
    public Guid? CreatedBy { get; set; }
}
