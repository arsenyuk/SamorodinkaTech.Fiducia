namespace SamorodinkaTech.Fiducia.Domain.Models.Spark;

/// <summary>
/// Структура компании из СПАРК — SOAP-метод GetCompanyStructure.
/// Иерархия: материнская компания → текущая → дочерние.
/// </summary>
public class SparkCompanyStructure
{
    /// <summary>Материнская компания (если есть).</summary>
    public SparkCompanyStructureItem? Parent { get; init; }

    /// <summary>Текущая компания.</summary>
    public SparkCompanyStructureItem Current { get; init; } = default!;

    /// <summary>Дочерние компании.</summary>
    public List<SparkCompanyStructureItem> Children { get; init; } = new();

    /// <summary>Аффилированные лица (руководители, учредители).</summary>
    public List<SparkAffiliatedPerson> Affiliates { get; init; } = new();
}

/// <summary>
/// Элемент структуры компании.
/// </summary>
public class SparkCompanyStructureItem
{
    /// <summary>Наименование компании.</summary>
    public string Name { get; init; } = default!;

    /// <summary>ИНН.</summary>
    public string Inn { get; init; } = default!;

    /// <summary>ОГРН.</summary>
    public string? Ogrn { get; init; }

    /// <summary>Доля участия (в процентах).</summary>
    public decimal? SharePercent { get; init; }

    /// <summary>Дата вхождения в структуру.</summary>
    public DateTime? EntryDate { get; init; }

    /// <summary>Дата выхода из структуры.</summary>
    public DateTime? ExitDate { get; init; }

    /// <summary>Роль в структуре (материнская/дочерняя).</summary>
    public string? Role { get; init; }
}

/// <summary>
/// Аффилированное лицо (руководитель/учредитель).
/// </summary>
public class SparkAffiliatedPerson
{
    /// <summary>ФИО.</summary>
    public string FullName { get; init; } = default!;

    /// <summary>ИНН.</summary>
    public string? Inn { get; init; }

    /// <summary>Должность.</summary>
    public string? Position { get; init; }

    /// <summary>Тип связи: director/founder/participant.</summary>
    public string? RelationType { get; init; }

    /// <summary>Доля участия (если учредитель).</summary>
    public decimal? SharePercent { get; init; }
}
