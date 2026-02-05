namespace Aira.Infrastructure.Providers;

using Aira.Core.Interfaces;
using Aira.Core.Models.DTOs;

/// <summary>
/// Mock implementation of news provider with deterministic sentiment data.
/// Provides realistic NVIDIA news and generic fallback for other tickers.
/// </summary>
public class MockNewsProvider : INewsProvider
{
    /// <inheritdoc/>
    public Task<List<NewsItem>> GetRecentHeadlinesAsync(string ticker)
    {
        var news = ticker.ToUpperInvariant() switch
        {
            "NVDA" => CreateNvidiaNews(),
            _ => CreateGenericNews(ticker)
        };

        return Task.FromResult(news);
    }

    private static List<NewsItem> CreateNvidiaNews()
    {
        return new List<NewsItem>
        {
            new NewsItem
            {
                Headline = "NVIDIA Reports Record Revenue Driven by AI Demand",
                Summary = "NVIDIA announces quarterly revenue up 206% year-over-year, fueled by unprecedented demand for AI chips and data center solutions. CEO Jensen Huang highlights accelerated adoption of generative AI platforms.",
                PublishedAt = DateTimeOffset.UtcNow.AddDays(-2),
                SentimentHint = "positive",
                Source = "Mock Financial News",
                Url = "mock://news/nvda/record-revenue"
            },
            new NewsItem
            {
                Headline = "Tech Giants Compete for NVIDIA GPU Supply",
                Summary = "Major cloud providers and tech companies scramble to secure NVIDIA H100 and next-gen Blackwell chips. Supply constraints persist despite increased production capacity.",
                PublishedAt = DateTimeOffset.UtcNow.AddDays(-5),
                SentimentHint = "positive",
                Source = "Mock Tech Insider",
                Url = "mock://news/nvda/gpu-demand"
            },
            new NewsItem
            {
                Headline = "NVIDIA Faces Increased Competition from AMD and Custom AI Chips",
                Summary = "Analysts warn of growing competition as AMD ramps up MI300 production and tech giants develop proprietary AI accelerators. Market share concerns emerge for long-term outlook.",
                PublishedAt = DateTimeOffset.UtcNow.AddDays(-7),
                SentimentHint = "negative",
                Source = "Mock Market Watch",
                Url = "mock://news/nvda/competition"
            },
            new NewsItem
            {
                Headline = "Export Restrictions on AI Chips to China Impact NVIDIA",
                Summary = "U.S. government tightens export controls on advanced semiconductors. NVIDIA develops China-specific chips but faces revenue headwinds from regulatory constraints.",
                PublishedAt = DateTimeOffset.UtcNow.AddDays(-10),
                SentimentHint = "negative",
                Source = "Mock Reuters",
                Url = "mock://news/nvda/export-controls"
            },
            new NewsItem
            {
                Headline = "NVIDIA Announces Breakthrough in AI Model Training Efficiency",
                Summary = "New tensor core architecture promises 40% improvement in AI training performance. Partnership with leading AI labs to optimize next-generation large language models.",
                PublishedAt = DateTimeOffset.UtcNow.AddDays(-12),
                SentimentHint = "positive",
                Source = "Mock Tech Review",
                Url = "mock://news/nvda/ai-breakthrough"
            }
        };
    }

    private static List<NewsItem> CreateGenericNews(string ticker)
    {
        return new List<NewsItem>
        {
            new NewsItem
            {
                Headline = $"{ticker} Reports Quarterly Earnings",
                Summary = $"{ticker} announces quarterly results meeting analyst expectations with steady revenue growth.",
                PublishedAt = DateTimeOffset.UtcNow.AddDays(-3),
                SentimentHint = "neutral",
                Source = "Mock Business News",
                Url = $"mock://news/{ticker}/earnings"
            },
            new NewsItem
            {
                Headline = $"{ticker} Announces Strategic Partnership",
                Summary = $"{ticker} enters new partnership to expand market reach and product offerings.",
                PublishedAt = DateTimeOffset.UtcNow.AddDays(-8),
                SentimentHint = "positive",
                Source = "Mock Industry News",
                Url = $"mock://news/{ticker}/partnership"
            }
        };
    }
}
