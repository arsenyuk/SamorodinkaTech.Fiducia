using SamorodinkaTech.Fiducia.Domain.Models.DaData;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Client for DaData API interaction.
/// Provides organization data from EGRUL (up to 3 days delay).
/// </summary>
public interface IDaDataApiClient
{
    /// <summary>
    /// Search organization by INN, OGRN or name.
    /// </summary>
    /// <param name="query">INN, OGRN or company name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Company info or null if not found.</returns>
    Task<DaDataCompanyInfo?> FindCompanyAsync(
        string query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get financial balance for a company by INN.
    /// Requires DaData "Professional" tariff.
    /// </summary>
    /// <param name="inn">Company INN.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Balance data or null if not available.</returns>
    Task<DaDataBalance?> GetBalanceAsync(
        string inn,
        CancellationToken cancellationToken = default);
}
