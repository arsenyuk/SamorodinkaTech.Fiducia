namespace SamorodinkaTech.Fiducia.Domain.Models.CbrFinOrg;

/// <summary>
/// Результат поиска участников финансового рынка с пагинацией.
/// Соответствует элементу RecordSet в ответе Search / SearchByINNs / SearchByOGRNs.
/// </summary>
public class CbrFinOrgSearchResult
{
    /// <summary>Список найденных записей (DS).</summary>
    public List<CbrFinOrgRecord> Records { get; set; } = new();

    /// <summary>Успешность обработки запроса (IsSucess).</summary>
    public bool IsSuccess { get; set; }

    /// <summary>Общее количество страниц (TotalPages).</summary>
    public uint TotalPages { get; set; }

    /// <summary>Номер текущей страницы (CurrentPage).</summary>
    public uint CurrentPage { get; set; }

    /// <summary>Максимальное количество записей на странице (PageSize).</summary>
    public uint PageSize { get; set; }

    /// <summary>Общее количество найденных записей (TotalRows).</summary>
    public uint TotalRows { get; set; }

    /// <summary>Текст ошибки, если запрос не удался (Error).</summary>
    public string? Error { get; set; }
}
