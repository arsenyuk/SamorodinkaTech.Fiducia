namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Сервис инстанцирования шаблона организационных задач в реальные задачи.
/// Создаёт org_intents → org_stages → org_offers → org_tasks из шаблона с кодом FIRST_BOARD.
/// </summary>
public interface ITemplateInstantiationService
{
    /// <summary>
    /// Инстанцирует шаблон «Первое заседание» для указанного юрлица.
    /// Задачи, привязанные к должности СД, создаются только если эта должность есть в составе СД.
    /// </summary>
    /// <param name="ctx">Контекст БД (один на всю операцию).</param>
    /// <param name="legalEntityId">Идентификатор юрлица.</param>
    /// <param name="boardOfDirectorsId">Идентификатор совета директоров (для проверки должностей).</param>
    /// <returns>Количество созданных задач (0 — шаблон не найден или нет задач).</returns>
    Task<int> InstantiateFirstBoardAsync(
        IApplicationDbContext ctx,
        Guid legalEntityId,
        Guid? boardOfDirectorsId);
}
