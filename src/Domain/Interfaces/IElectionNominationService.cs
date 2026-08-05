namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Сервис работы с выдвижением кандидатов на должности СД:
/// рассылка уведомлений о сборе предложений и о необходимости подписать согласие.
/// </summary>
public interface IElectionNominationService
{
    /// <summary>
    /// Рассылает уведомления о необходимости подписать согласие
    /// всем подтверждённым кандидатам по указанному предложению.
    /// </summary>
    /// <param name="proposalId">Идентификатор предложения кандидатур (ElectionProposal).</param>
    /// <param name="legalEntityName">Полное наименование юридического лица.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Количество отправленных уведомлений.</returns>
    Task<int> SendConsentNotificationsAsync(
        Guid proposalId,
        string legalEntityName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Рассылает уведомления о сборе предложений кандидатур
    /// всем членам Совета директоров.
    /// </summary>
    /// <param name="boardOfDirectorsId">Идентификатор Совета директоров.</param>
    /// <param name="position">Должность (CHAIR / DEPUTY_CHAIR).</param>
    /// <param name="legalEntityName">Полное наименование юридического лица.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Количество отправленных уведомлений.</returns>
    Task<int> SendNominationNotificationsAsync(
        Guid boardOfDirectorsId,
        string position,
        string legalEntityName,
        CancellationToken cancellationToken = default);
}
