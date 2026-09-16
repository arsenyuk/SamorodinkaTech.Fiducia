namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Сервис сохранения состава совета директоров без loopback HTTP.
/// Заменяет POST /save эндпоинт BoardSetupEndpoints.
/// </summary>
public interface IBoardSetupService
{
    /// <summary>
    /// Сохраняет состав СД: удаляет старые роли и создаёт новые назначения.
    /// </summary>
    /// <param name="model">Модель сохранения.</param>
    /// <param name="ct">Токен отмены.</param>
    Task SaveAsync(BoardSetupSaveModel model, CancellationToken ct = default);
}

/// <summary>
/// Модель для сохранения состава СД.
/// </summary>
public class BoardSetupSaveModel
{
    /// <summary>Идентификатор юридического лица.</summary>
    public Guid LegalEntityId { get; init; }

    /// <summary>Идентификатор текущего пользователя.</summary>
    public Guid UserId { get; init; }

    /// <summary>Список назначений ролей.</summary>
    public List<BoardSetupAssignmentModel> Assignments { get; init; } = new();
}

/// <summary>
/// Модель назначения роли участника СД.
/// </summary>
public class BoardSetupAssignmentModel
{
    /// <summary>Идентификатор участника.</summary>
    public Guid ParticipantId { get; init; }

    /// <summary>Код роли (BOARD_CHAIRMAN, BOARD_SECRETARY и т.д.).</summary>
    public string RoleCode { get; init; } = default!;
}
