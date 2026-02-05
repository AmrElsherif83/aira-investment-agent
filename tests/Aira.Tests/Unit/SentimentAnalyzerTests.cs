using Aira.Core.Agent.Helpers;
using Aira.Core.Models.DTOs;
using Xunit;

namespace Aira.Tests.Unit;

public class SentimentAnalyzerTests
{
    #region Test Data Helpers

    private static NewsItem CreateNewsItem(
        string sentiment,
        DateTime? publishedAt = null)
    {
        return new NewsItem
        {
            Headline = $"Test headline - {sentiment}",
            Summary = $"Test summary with {sentiment} tone",
            SentimentHint = sentiment,
            PublishedAt = publishedAt ?? DateTime.UtcNow,
            Url = "https://test.com/article"
        };
    }

    #endregion

    #region Positive Keywords Increase Score

    [Fact]
    public void AnalyzeSentiment_PositiveNews_ReturnsPositiveScore()
    {
        // Arrange
        var news = new List<NewsItem>
        {
            CreateNewsItem("positive"),
            CreateNewsItem("positive"),
            CreateNewsItem("positive")
        };

        // Act
        var score = SentimentAnalyzer.AnalyzeSentiment(news);

        // Assert
        Assert.True(score > 0, $"Expected positive score but got {score}");
        Assert.True(score <= 1.0, "Score should not exceed 1.0");
    }

    [Fact]
    public void AnalyzeSentiment_SinglePositiveNews_ReturnsOne()
    {
        // Arrange
        var news = new List<NewsItem>
        {
            CreateNewsItem("positive", DateTime.UtcNow.AddDays(-1))
        };

        // Act
        var score = SentimentAnalyzer.AnalyzeSentiment(news);

        // Assert
        Assert.Equal(1.0, score, precision: 2);
    }

    #endregion

    #region Negative Keywords Decrease Score

    [Fact]
    public void AnalyzeSentiment_NegativeNews_ReturnsNegativeScore()
    {
        // Arrange
        var news = new List<NewsItem>
        {
            CreateNewsItem("negative"),
            CreateNewsItem("negative"),
            CreateNewsItem("negative")
        };

        // Act
        var score = SentimentAnalyzer.AnalyzeSentiment(news);

        // Assert
        Assert.True(score < 0, $"Expected negative score but got {score}");
        Assert.True(score >= -1.0, "Score should not be less than -1.0");
    }

    [Fact]
    public void AnalyzeSentiment_SingleNegativeNews_ReturnsNegativeOne()
    {
        // Arrange
        var news = new List<NewsItem>
        {
            CreateNewsItem("negative", DateTime.UtcNow.AddDays(-1))
        };

        // Act
        var score = SentimentAnalyzer.AnalyzeSentiment(news);

        // Assert
        Assert.Equal(-1.0, score, precision: 2);
    }

    #endregion

    #region Mixed Sentiment

    [Fact]
    public void AnalyzeSentiment_MixedNews_ReturnsWeightedAverage()
    {
        // Arrange - All recent news with equal weights
        var now = DateTime.UtcNow;
        var news = new List<NewsItem>
        {
            CreateNewsItem("positive", now.AddDays(-1)),  // +1.0 * 1.0 weight
            CreateNewsItem("negative", now.AddDays(-1)),  // -1.0 * 1.0 weight
            CreateNewsItem("neutral", now.AddDays(-1))    //  0.0 * 1.0 weight
        };

        // Act
        var score = SentimentAnalyzer.AnalyzeSentiment(news);

        // Assert
        // Expected: (1.0 + (-1.0) + 0.0) / 3.0 = 0.0
        Assert.Equal(0.0, score, precision: 2);
    }

    [Fact]
    public void AnalyzeSentiment_MorePositiveThanNegative_ReturnsPositiveScore()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var news = new List<NewsItem>
        {
            CreateNewsItem("positive", now.AddDays(-1)),
            CreateNewsItem("positive", now.AddDays(-1)),
            CreateNewsItem("negative", now.AddDays(-1))
        };

        // Act
        var score = SentimentAnalyzer.AnalyzeSentiment(news);

        // Assert
        // Expected: (1.0 + 1.0 + (-1.0)) / 3.0 ? 0.33
        Assert.True(score > 0, $"Expected positive score with 2 positive and 1 negative, got {score}");
        Assert.Equal(0.33, score, precision: 1);
    }

    #endregion

    #region Recency Weighting

    [Fact]
    public void AnalyzeSentiment_RecentNewsHasHigherWeight()
    {
        // Arrange
        var now = DateTime.UtcNow;
        
        // Recent positive news
        var recentNews = new List<NewsItem>
        {
            CreateNewsItem("positive", now.AddDays(-1)),    // Weight: 1.0
            CreateNewsItem("negative", now.AddDays(-20))    // Weight: 0.4 (14-30 days)
        };
        
        // Old positive news
        var oldNews = new List<NewsItem>
        {
            CreateNewsItem("positive", now.AddDays(-20)),   // Weight: 0.4
            CreateNewsItem("negative", now.AddDays(-1))     // Weight: 1.0
        };

        // Act
        var recentScore = SentimentAnalyzer.AnalyzeSentiment(recentNews);
        var oldScore = SentimentAnalyzer.AnalyzeSentiment(oldNews);

        // Assert
        // Recent positive should result in positive score: (1.0*1.0 + (-1.0)*0.4) / 1.4 ? 0.43
        Assert.True(recentScore > 0, $"Recent positive should dominate, got {recentScore}");
        
        // Old positive should result in negative score: (1.0*0.4 + (-1.0)*1.0) / 1.4 ? -0.43
        Assert.True(oldScore < 0, $"Recent negative should dominate, got {oldScore}");
    }

    [Theory]
    [InlineData(1)]   // <3 days: weight 1.0
    [InlineData(5)]   // 3-7 days: weight 0.8
    [InlineData(10)]  // 7-14 days: weight 0.6
    [InlineData(20)]  // 14-30 days: weight 0.4
    [InlineData(35)]  // >30 days: weight 0.2
    public void AnalyzeSentiment_AppliesCorrectRecencyWeight(int daysAgo)
    {
        // Arrange
        var news = new List<NewsItem>
        {
            CreateNewsItem("positive", DateTime.UtcNow.AddDays(-daysAgo))
        };

        // Act
        var score = SentimentAnalyzer.AnalyzeSentiment(news);

        // Assert
        // For single positive news: score = (1.0 * weight) / weight = 1.0
        // The weight affects multiple news items differently, but single item always = sentiment value
        Assert.Equal(1.0, score, precision: 2);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void AnalyzeSentiment_EmptyList_ReturnsZero()
    {
        // Arrange
        var news = new List<NewsItem>();

        // Act
        var score = SentimentAnalyzer.AnalyzeSentiment(news);

        // Assert
        Assert.Equal(0.0, score);
    }

    [Fact]
    public void AnalyzeSentiment_NullList_ReturnsZero()
    {
        // Arrange
        List<NewsItem>? news = null;

        // Act
        var score = SentimentAnalyzer.AnalyzeSentiment(news!);

        // Assert
        Assert.Equal(0.0, score);
    }

    [Fact]
    public void AnalyzeSentiment_AllNeutralNews_ReturnsZero()
    {
        // Arrange
        var news = new List<NewsItem>
        {
            CreateNewsItem("neutral"),
            CreateNewsItem("neutral"),
            CreateNewsItem("neutral")
        };

        // Act
        var score = SentimentAnalyzer.AnalyzeSentiment(news);

        // Assert
        Assert.Equal(0.0, score);
    }

    [Fact]
    public void AnalyzeSentiment_UnrecognizedSentiment_TreatedAsNeutral()
    {
        // Arrange
        var news = new List<NewsItem>
        {
            CreateNewsItem("unknown"),
            CreateNewsItem("invalid"),
            CreateNewsItem("positive")
        };

        // Act
        var score = SentimentAnalyzer.AnalyzeSentiment(news);

        // Assert
        // Expected: (0.0 + 0.0 + 1.0) / 3.0 ? 0.33
        Assert.Equal(0.33, score, precision: 1);
    }

    #endregion

    #region Count Sentiments

    [Fact]
    public void CountSentiments_CorrectlyCategorizesSentiment()
    {
        // Arrange
        var news = new List<NewsItem>
        {
            CreateNewsItem("positive"),
            CreateNewsItem("positive"),
            CreateNewsItem("negative"),
            CreateNewsItem("neutral")
        };

        // Act
        var (positive, neutral, negative) = SentimentAnalyzer.CountSentiments(news);

        // Assert
        Assert.Equal(2, positive);
        Assert.Equal(1, neutral);
        Assert.Equal(1, negative);
    }

    [Fact]
    public void CategorizeSentiment_PositiveScore_ReturnsPositive()
    {
        // Act & Assert
        Assert.Equal("Positive", SentimentAnalyzer.CategorizeSentiment(0.5));
        Assert.Equal("Very Positive", SentimentAnalyzer.CategorizeSentiment(0.7));
    }

    [Fact]
    public void CategorizeSentiment_NegativeScore_ReturnsNegative()
    {
        // Act & Assert
        Assert.Equal("Negative", SentimentAnalyzer.CategorizeSentiment(-0.5));
        Assert.Equal("Very Negative", SentimentAnalyzer.CategorizeSentiment(-0.7));
    }

    [Fact]
    public void CategorizeSentiment_ZeroScore_ReturnsNeutral()
    {
        // Act & Assert
        Assert.Equal("Neutral", SentimentAnalyzer.CategorizeSentiment(0.0));
    }

    #endregion
}

