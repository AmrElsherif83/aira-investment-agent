using Aira.Core.Agent.Helpers;
using Aira.Core.Models;
using Xunit;

namespace Aira.Tests.Unit;

public class SignalGeneratorTests
{
    #region Signal Boundary Tests - Bullish

    [Theory]
    [InlineData(65.0, 0.70)]
    [InlineData(70.0, 0.60)]
    [InlineData(85.0, 0.80)]
    [InlineData(100.0, 0.95)]
    public void GenerateSignal_ScoreAbove65_ReturnsBullish(double score, double confidence)
    {
        // Arrange
        var compositeScore = score;

        // Act
        var signal = SignalGenerator.GenerateSignal(compositeScore, confidence);

        // Assert
        Assert.Equal("Bullish", signal);
    }

    [Fact]
    public void GenerateSignal_ScoreExactly65_ReturnsBullish()
    {
        // Arrange
        var compositeScore = 65.0;
        var confidence = 0.70;

        // Act
        var signal = SignalGenerator.GenerateSignal(compositeScore, confidence);

        // Assert
        Assert.Equal("Bullish", signal);
    }

    #endregion

    #region Signal Boundary Tests - Neutral

    [Theory]
    [InlineData(45.0, 0.70)]
    [InlineData(50.0, 0.60)]
    [InlineData(55.0, 0.65)]
    [InlineData(64.99, 0.75)]
    public void GenerateSignal_ScoreBetween45And65_ReturnsNeutral(double score, double confidence)
    {
        // Arrange
        var compositeScore = score;

        // Act
        var signal = SignalGenerator.GenerateSignal(compositeScore, confidence);

        // Assert
        Assert.Equal("Neutral", signal);
    }

    [Fact]
    public void GenerateSignal_ScoreExactly45_ReturnsNeutral()
    {
        // Arrange
        var compositeScore = 45.0;
        var confidence = 0.70;

        // Act
        var signal = SignalGenerator.GenerateSignal(compositeScore, confidence);

        // Assert
        Assert.Equal("Neutral", signal);
    }

    #endregion

    #region Signal Boundary Tests - Bearish

    [Theory]
    [InlineData(44.99, 0.70)]
    [InlineData(40.0, 0.60)]
    [InlineData(30.0, 0.65)]
    [InlineData(20.0, 0.75)]
    [InlineData(0.0, 0.80)]
    public void GenerateSignal_ScoreBelow45_ReturnsBearish(double score, double confidence)
    {
        // Arrange
        var compositeScore = score;

        // Act
        var signal = SignalGenerator.GenerateSignal(compositeScore, confidence);

        // Assert
        Assert.Equal("Bearish", signal);
    }

    #endregion

    #region Confidence Override - Low Confidence Forces Neutral

    [Theory]
    [InlineData(70.0, 0.39)]  // High score but low confidence
    [InlineData(85.0, 0.35)]
    [InlineData(95.0, 0.20)]
    public void GenerateSignal_HighScoreLowConfidence_ReturnsNeutral(double score, double confidence)
    {
        // Arrange
        var compositeScore = score;

        // Act
        var signal = SignalGenerator.GenerateSignal(compositeScore, confidence);

        // Assert
        Assert.Equal("Neutral", signal);
    }

    [Theory]
    [InlineData(30.0, 0.39)]  // Low score and low confidence
    [InlineData(25.0, 0.35)]
    [InlineData(15.0, 0.20)]
    public void GenerateSignal_LowScoreLowConfidence_ReturnsNeutral(double score, double confidence)
    {
        // Arrange
        var compositeScore = score;

        // Act
        var signal = SignalGenerator.GenerateSignal(compositeScore, confidence);

        // Assert
        Assert.Equal("Neutral", signal);
    }

    [Fact]
    public void GenerateSignal_ConfidenceExactly40_DoesNotOverride()
    {
        // Arrange - Score 70 should be Bullish
        var compositeScore = 70.0;
        var confidence = 0.40;

        // Act
        var signal = SignalGenerator.GenerateSignal(compositeScore, confidence);

        // Assert
        Assert.Equal("Bullish", signal);
    }

    [Fact]
    public void GenerateSignal_ConfidenceBelow40_OverridesToNeutral()
    {
        // Arrange - Score 70 would be Bullish
        var compositeScore = 70.0;
        var confidence = 0.3999;

        // Act
        var signal = SignalGenerator.GenerateSignal(compositeScore, confidence);

        // Assert
        Assert.Equal("Neutral", signal);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void GenerateSignal_NegativeScore_ReturnsBearish()
    {
        // Arrange
        var compositeScore = -10.0;
        var confidence = 0.70;

        // Act
        var signal = SignalGenerator.GenerateSignal(compositeScore, confidence);

        // Assert
        Assert.Equal("Bearish", signal);
    }

    [Fact]
    public void GenerateSignal_ScoreAbove100_ReturnsBullish()
    {
        // Arrange
        var compositeScore = 110.0;
        var confidence = 0.70;

        // Act
        var signal = SignalGenerator.GenerateSignal(compositeScore, confidence);

        // Assert
        Assert.Equal("Bullish", signal);
    }

    #endregion
}
