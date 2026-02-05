namespace Aira.Core.Agent.Helpers;

using Aira.Core.Models;
using Aira.Core.Models.DTOs;

/// <summary>
/// Helper class for calculating investment scores using deterministic rules.
/// </summary>
public static class ScoreCalculator
{
    // Scoring weights
    private const double FinancialWeight = 0.40;
    private const double SentimentWeight = 0.30;
    private const double RiskWeight = 0.30;

    /// <summary>
    /// Calculates a comprehensive score breakdown from gathered data.
    /// </summary>
    public static ScoreBreakdown CalculateScores(
        FinancialSnapshot? financialData,
        double sentimentScore,
        List<RiskItem> risks)
    {
        var financialScore = CalculateFinancialScore(financialData);
        var riskScore = CalculateRiskScore(risks);

        // Sentiment score is already -1 to +1, normalize to 0-100
        var normalizedSentimentScore = (sentimentScore + 1.0) * 50.0;

        var compositeScore = 
            (financialScore * FinancialWeight) +
            (normalizedSentimentScore * SentimentWeight) +
            (riskScore * RiskWeight);

        return new ScoreBreakdown
        {
            FinancialScore = financialScore,
            SentimentScore = normalizedSentimentScore,
            MarketScore = riskScore, // Using risk score as market assessment
            CompositeScore = compositeScore,
            FinancialWeight = FinancialWeight,
            SentimentWeight = SentimentWeight,
            MarketWeight = RiskWeight
        };
    }

    /// <summary>
    /// Calculates financial health score (0-100) from financial snapshot.
    /// </summary>
    private static double CalculateFinancialScore(FinancialSnapshot? data)
    {
        if (data == null)
        {
            return 50.0; // Neutral if no data
        }

        double score = 50.0; // Start neutral

        // Revenue growth contribution (max +25)
        if (data.RevenueGrowthYoY > 50)
            score += 25;
        else if (data.RevenueGrowthYoY > 20)
            score += 20;
        else if (data.RevenueGrowthYoY > 10)
            score += 15;
        else if (data.RevenueGrowthYoY > 5)
            score += 10;
        else if (data.RevenueGrowthYoY > 0)
            score += 5;
        else
            score -= 10; // Negative growth

        // Profit margin contribution (max +15)
        if (data.ProfitMargin > 40)
            score += 15;
        else if (data.ProfitMargin > 30)
            score += 12;
        else if (data.ProfitMargin > 20)
            score += 10;
        else if (data.ProfitMargin > 10)
            score += 5;
        else if (data.ProfitMargin > 0)
            score += 2;
        else
            score -= 10; // Negative margin

        // ROE contribution (max +10)
        if (data.ReturnOnEquity > 50)
            score += 10;
        else if (data.ReturnOnEquity > 30)
            score += 8;
        else if (data.ReturnOnEquity > 20)
            score += 6;
        else if (data.ReturnOnEquity > 15)
            score += 4;
        else if (data.ReturnOnEquity > 10)
            score += 2;

        // Cap between 0 and 100
        return Math.Max(0, Math.Min(100, score));
    }

    /// <summary>
    /// Calculates risk-adjusted score (0-100) from risk items.
    /// Higher score means lower risk / better market position.
    /// </summary>
    private static double CalculateRiskScore(List<RiskItem> risks)
    {
        if (risks == null || risks.Count == 0)
        {
            return 75.0; // Assume moderate-high if no risks identified
        }

        double score = 100.0; // Start optimistic

        // Deduct points based on risk severity
        foreach (var risk in risks)
        {
            var severity = risk.Severity?.ToLowerInvariant();
            
            var deduction = severity switch
            {
                "critical" => 20.0,
                "high" => 12.0,
                "medium" => 6.0,
                "low" => 2.0,
                _ => 4.0 // Unknown severity
            };

            score -= deduction;
        }

        // Cap between 0 and 100
        return Math.Max(0, Math.Min(100, score));
    }

    /// <summary>
    /// Calculates initial confidence based on score and data quality.
    /// </summary>
    public static double CalculateConfidence(ScoreBreakdown scores, double dataCompleteness)
    {
        // Base confidence on composite score variance and data quality
        double baseConfidence = 0.70; // Start at 70%

        // Adjust based on data completeness
        baseConfidence *= dataCompleteness;

        // Reduce confidence if scores are contradictory
        var scoreVariance = CalculateScoreVariance(scores);
        if (scoreVariance > 30)
        {
            baseConfidence *= 0.85; // High variance -> reduce confidence
        }
        else if (scoreVariance > 20)
        {
            baseConfidence *= 0.92;
        }

        // Extreme composite scores get slightly higher confidence
        if (scores.CompositeScore > 75 || scores.CompositeScore < 30)
        {
            baseConfidence *= 1.05;
        }

        return Math.Max(0.0, Math.Min(1.0, baseConfidence));
    }

    /// <summary>
    /// Calculates variance between component scores to detect contradictions.
    /// </summary>
    private static double CalculateScoreVariance(ScoreBreakdown scores)
    {
        var scores_list = new[] 
        { 
            scores.FinancialScore, 
            scores.SentimentScore, 
            scores.MarketScore 
        };

        var mean = scores_list.Average();
        var variance = scores_list.Select(s => Math.Pow(s - mean, 2)).Average();
        
        return Math.Sqrt(variance);
    }
}
