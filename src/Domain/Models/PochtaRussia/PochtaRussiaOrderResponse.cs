namespace SamorodinkaTech.Fiducia.Domain.Models.PochtaRussia;

/// <summary>
/// Ответ на запрос создания заказа (PUT /1.0/user/backlog).
/// </summary>
public class PochtaRussiaOrderResponse
{
    /// <summary>Список успешно созданных внутренних идентификаторов отправлений.</summary>
    public List<long> ResultIds { get; init; } = new();

    /// <summary>Список ошибок.</summary>
    public List<PochtaRussiaOrderError> Errors { get; init; } = new();
}

/// <summary>
/// Ошибка при создании заказа.
/// </summary>
public class PochtaRussiaOrderError
{
    /// <summary>Список кодов ошибок.</summary>
    public List<PochtaRussiaErrorCode> ErrorCodes { get; init; } = new();

    /// <summary>Индекс в массиве заказов.</summary>
    public int? Position { get; init; }
}

/// <summary>
/// Код ошибки.
/// </summary>
public class PochtaRussiaErrorCode
{
    /// <summary>Код ошибки (например, ILLEGAL_MASS_EXCESS).</summary>
    public string Code { get; init; } = default!;

    /// <summary>Описание ошибки.</summary>
    public string? Description { get; init; }

    /// <summary>Детальное описание ошибки.</summary>
    public string? Details { get; init; }

    /// <summary>Индекс в массиве.</summary>
    public int? Position { get; init; }
}
