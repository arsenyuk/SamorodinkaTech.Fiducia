namespace SamorodinkaTech.Fiducia.Domain.Validation;

/// <summary>
/// Результат серверной валидации сохранения юридического лица.
/// </summary>
public class LegalEntitySaveValidationResult
{
    private readonly List<string> _errors = new();

    /// <summary>True, если ошибок нет.</summary>
    public bool IsValid => _errors.Count == 0;

    /// <summary>Список ошибок валидации.</summary>
    public IReadOnlyList<string> Errors => _errors;

    /// <summary>Добавляет ошибку.</summary>
    public void AddError(string error)
    {
        _errors.Add(error);
    }

    /// <summary>Создаёт успешный результат.</summary>
    public static LegalEntitySaveValidationResult Success() => new();

    /// <summary>Создаёт результат с одной ошибкой.</summary>
    public static LegalEntitySaveValidationResult Failure(string error)
    {
        var r = new LegalEntitySaveValidationResult();
        r.AddError(error);
        return r;
    }
}
