namespace Aira.Core.Interfaces;

using Aira.Core.Models.DTOs;

/// <summary>
/// Provides news and sentiment data for companies.
/// Implementation may use mock data or real news APIs.
/// </summary>
public interface INewsProvider
{
    /// <summary>
    /// Retrieves recent news headlines and articles for the specified ticker.
    /// </summary>
    /// <param name="ticker">Stock ticker symbol.</param>
    /// <returns>List of news items with headlines, summaries, and sentiment hints.</returns>
    Task<List<NewsItem>> GetRecentHeadlinesAsync(string ticker);
}
