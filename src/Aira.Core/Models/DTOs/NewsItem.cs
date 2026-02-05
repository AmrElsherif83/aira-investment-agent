namespace Aira.Core.Models.DTOs;

/// <summary>
/// Represents a news article or headline related to a company.
/// </summary>
public class NewsItem
{
    /// <summary>
    /// News headline or title.
    /// </summary>
    public required string Headline { get; set; }

    /// <summary>
    /// Brief summary or snippet of the article content.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Publication date and time.
    /// </summary>
    public DateTimeOffset PublishedAt { get; set; }

    /// <summary>
    /// Optional sentiment hint: "positive", "negative", or "neutral".
    /// </summary>
    public string? SentimentHint { get; set; }

    /// <summary>
    /// URL to the full article.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// News source or publication name.
    /// </summary>
    public string? Source { get; set; }
}
