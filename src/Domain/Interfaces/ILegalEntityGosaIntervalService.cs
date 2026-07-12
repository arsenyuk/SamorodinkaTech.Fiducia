namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Сервис расчёта и валидации интервала проведения ГОСА для ЮЛ по ОПФ/уставу.
/// </summary>
public interface ILegalEntityGosaIntervalService
{
    /// <summary>
    /// ПАО — Публичное акционерное общество (Public Joint-Stock Company).
    /// </summary>
    bool IsPjsc(string? okopfCode);

    /// <summary>
    /// Возвращает, является ли ОПФ ООО.
    /// </summary>
    bool IsLlc(string? okopfCode);

    /// <summary>
    /// Возвращает стандартный интервал ГОСА по бизнес-правилу: 01.03–30.06.
    /// </summary>
    (DateOnly start, DateOnly end) GetDefaultWindow();

    /// <summary>
    /// Возвращает интервал проведения годового общего собрания с учётом организационно-правовой формы.
    /// Для ПАО и НАО: 01.03–30.06 (ст. 47 208-ФЗ).
    /// Для ООО: 01.03–30.04 (ст. 34 14-ФЗ).
    /// </summary>
    (DateOnly start, DateOnly end) GetWindowForOkopf(string? okopfCode);

    /// <summary>
    /// Проверяет, допустим ли интервал для указанной ОПФ.
    /// Для ПАО: в пределах 01.03–30.06 и start <= end.
    /// Для НАО: разрешён только ровно 01.03–30.06.
    /// </summary>
    bool ValidateForOkopf(string? okopfCode, DateOnly start, DateOnly end);
}
