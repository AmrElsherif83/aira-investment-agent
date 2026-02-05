namespace Aira.Core.Agent.Helpers;

using Aira.Core.Models.DTOs;

/// <summary>
/// Helper class for analyzing news sentiment using deterministic rules.
/// </summary>
public static class SentimentAnalyzer
{
    /// <summary>
    /// Analyzes a collection of news items and returns an aggregate sentiment score.
    /// </summary>
    /// <param name="newsItems">List of news items to analyze.</param>
    /// <returns>Sentiment score from -1.0 (very negative) to +1.0 (very positive).</returns>
    public static double AnalyzeSentiment(List<NewsItem> newsItems)
    {
        if (newsItems == null || newsItems.Count == 0)
        {
            return 0.0; // Neutral if no news
        }

        double totalScore = 0.0;
        double totalWeight = 0.0;

        foreach (var item in newsItems)
        {
            // Weight more recent news higher
            var recencyWeight = CalculateRecencyWeight(item.PublishedAt);
            var sentimentScore = ParseSentimentHint(item.SentimentHint);

            totalScore += sentimentScore * recencyWeight;
            totalWeight += recencyWeight;
        }

        return totalWeight > 0 ? totalScore / totalWeight : 0.0;
    }

    /// <summary>
    /// Calculates a recency weight for news items.
    /// More recent news has higher weight.
    /// </summary>
    private static double CalculateRecencyWeight(DateTimeOffset publishedAt)
    {
        var daysOld = (DateTimeOffset.UtcNow - publishedAt).TotalDays;

        // Exponential decay: recent news has more weight
        if (daysOld < 0) daysOld = 0;
        if (daysOld < 3) return 1.0;      // Last 3 days: full weight
        if (daysOld < 7) return 0.8;      // 3-7 days: 80%
        if (daysOld < 14) return 0.6;     // 7-14 days: 60%
        if (daysOld < 30) return 0.4;     // 14-30 days: 40%
        return 0.2;                        // Older: 20%
    }

    /// <summary>
    /// Parses sentiment hint string into numeric score.
    /// </summary>
    private static double ParseSentimentHint(string? sentimentHint)
    {
        if (string.IsNullOrWhiteSpace(sentimentHint))
        {
            return 0.0; // Neutral
        }

        return sentimentHint.ToLowerInvariant() switch
        {
            "positive" => 1.0,
            "negative" => -1.0,
            "neutral" => 0.0,
            _ => 0.0 // Default to neutral
        };
    }

    /// <summary>
    /// Categorizes sentiment score into descriptive category.
    /// </summary>
    public static string CategorizeSentiment(double sentimentScore)
    {
        return sentimentScore switch
        {
            >= 0.6 => "Very Positive",
            >= 0.2 => "Positive",
            >= -0.2 => "Neutral",
            >= -0.6 => "Negative",
            _ => "Very Negative"
        };
    }

    /// <summary>
    /// Extracts positive and negative news counts for insight generation.
    /// </summary>
    public static (int Positive, int Negative, int Neutral) CountSentiments(List<NewsItem> newsItems)
    {
        int positive = 0, negative = 0, neutral = 0;

        foreach (var item in newsItems)
        {
            var hint = item.SentimentHint?.ToLowerInvariant();
            if (hint == "positive") positive++;
            else if (hint == "negative") negative++;
            else neutral++;
        }

        return (positive, negative, neutral);
    }
}
