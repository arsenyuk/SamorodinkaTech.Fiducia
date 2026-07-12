namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Сервис инстанцирования шаблона организационных задач в реальные задачи.
/// Создаёт org_intents → org_stages → org_tasks из шаблона с заданным кодом.
/// Каждый шаблонный офер порождает ровно одну задачу.
/// </summary>
public interface ITemplateInstantiationService
{
    /// <summary>
    /// Инстанцирует шаблон по коду (GOSA, OOSU, VOSA, VOSU, FIRST_BOARD, BOARD_MEETING) для указанного юрлица.
    /// Задачи с предикатами создаются только при выполнении условий конкретного ЮЛ.
    /// </summary>
    /// <param name="ctx">Контекст БД (один на всю операцию).</param>
    /// <param name="code">Код шаблона: GOSA, OOSU, VOSA, VOSU, FIRST_BOARD, BOARD_MEETING.</param>
    /// <param name="legalEntityId">Идентификатор юрлица.</param>
    /// <param name="boardOfDirectorsId">Идентификатор совета директоров (null = без проверки должностей СД).</param>
    /// <returns>Количество созданных задач (0 — шаблон не найден или нет задач).</returns>
    Task<int> InstantiateAsync(
        IApplicationDbContext ctx,
        string code,
        Guid legalEntityId,
        Guid? boardOfDirectorsId);

    /// <summary>
    /// Инстанцирует шаблон «Первое заседание» для указанного юрлица.
    /// Задачи, привязанные к должности СД, создаются только если эта должность есть в составе СД.
    /// </summary>
    Task<int> InstantiateFirstBoardAsync(
        IApplicationDbContext ctx,
        Guid legalEntityId,
        Guid? boardOfDirectorsId);
}
