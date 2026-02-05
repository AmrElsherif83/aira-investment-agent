namespace Aira.Core.Interfaces;

using Aira.Core.Models.DTOs;

/// <summary>
/// Provides risk assessment data for companies.
/// Implementation may use mock data or real risk analysis services.
/// </summary>
public interface IRiskProvider
{
    /// <summary>
    /// Retrieves known risk factors for the specified ticker.
    /// </summary>
    /// <param name="ticker">Stock ticker symbol.</param>
    /// <returns>List of identified risk items with descriptions and severity levels.</returns>
    Task<List<RiskItem>> GetKnownRisksAsync(string ticker);
}
