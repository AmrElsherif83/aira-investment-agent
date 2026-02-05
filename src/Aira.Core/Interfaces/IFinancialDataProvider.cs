namespace Aira.Core.Interfaces;

using Aira.Core.Models.DTOs;

/// <summary>
/// Provides financial data for companies.
/// Implementation may use mock data or real financial APIs.
/// </summary>
public interface IFinancialDataProvider
{
    /// <summary>
    /// Retrieves a structured financial snapshot for the specified ticker.
    /// </summary>
    /// <param name="ticker">Stock ticker symbol.</param>
    /// <returns>Financial snapshot containing key metrics and ratios.</returns>
    Task<FinancialSnapshot> GetFinancialSnapshotAsync(string ticker);
}
