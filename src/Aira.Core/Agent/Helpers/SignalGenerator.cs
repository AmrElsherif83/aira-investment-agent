namespace Aira.Core.Agent.Helpers;

using Aira.Core.Models;

/// <summary>
/// Helper class for generating investment signals from scores.
/// </summary>
public static class SignalGenerator
{
    /// <summary>
    /// Generates an investment signal from composite score and confidence.
    /// </summary>
    /// <param name="compositeScore">Composite score (0-100).</param>
    /// <param name="confidence">Confidence level (0.0-1.0).</param>
    /// <returns>Investment signal: "Bullish", "Neutral", or "Bearish".</returns>
    public static string GenerateSignal(double compositeScore, double confidence)
    {
        // If confidence is too low, default to Neutral
        if (confidence < 0.40)
        {
            return "Neutral";
        }

        // Score-based signal generation
        if (compositeScore >= 65)
        {
            return "Bullish";
        }
        else if (compositeScore >= 45)
        {
            return "Neutral";
        }
        else
        {
            return "Bearish";
        }
    }

    /// <summary>
    /// Generates a detailed investment thesis based on scores and gathered data.
    /// </summary>
    public static string GenerateThesis(
        string ticker,
        string companyName,
        ScoreBreakdown scores,
        string signal,
        double confidence)
    {
        var thesisBuilder = new System.Text.StringBuilder();

        thesisBuilder.AppendLine($"Investment Analysis for {companyName} ({ticker}):");
        thesisBuilder.AppendLine();

        // Overall assessment
        thesisBuilder.AppendLine($"Signal: {signal} (Confidence: {confidence:P0})");
        thesisBuilder.AppendLine($"Composite Score: {scores.CompositeScore:F1}/100");
        thesisBuilder.AppendLine();

        // Component breakdown
        thesisBuilder.AppendLine("Score Breakdown:");
        thesisBuilder.AppendLine($"- Financial Health: {scores.FinancialScore:F1}/100 (Weight: {scores.FinancialWeight:P0})");
        thesisBuilder.AppendLine($"- Market Sentiment: {scores.SentimentScore:F1}/100 (Weight: {scores.SentimentWeight:P0})");
        thesisBuilder.AppendLine($"- Risk Assessment: {scores.MarketScore:F1}/100 (Weight: {scores.MarketWeight:P0})");
        thesisBuilder.AppendLine();

        // Narrative based on signal
        if (signal == "Bullish")
        {
            thesisBuilder.AppendLine("Investment Rationale:");
            thesisBuilder.AppendLine($"The analysis indicates a favorable investment opportunity for {companyName}. ");
            
            if (scores.FinancialScore > 70)
                thesisBuilder.AppendLine("Strong financial fundamentals support growth prospects. ");
            
            if (scores.SentimentScore > 60)
                thesisBuilder.AppendLine("Positive market sentiment and news flow reinforce the bullish outlook. ");
            
            if (scores.MarketScore > 65)
                thesisBuilder.AppendLine("Risk profile is manageable relative to potential upside. ");
        }
        else if (signal == "Bearish")
        {
            thesisBuilder.AppendLine("Investment Rationale:");
            thesisBuilder.AppendLine($"The analysis suggests caution regarding {companyName}. ");
            
            if (scores.FinancialScore < 40)
                thesisBuilder.AppendLine("Financial fundamentals show concerning trends. ");
            
            if (scores.SentimentScore < 40)
                thesisBuilder.AppendLine("Negative market sentiment and news flow indicate headwinds. ");
            
            if (scores.MarketScore < 40)
                thesisBuilder.AppendLine("Risk factors present significant challenges. ");
        }
        else // Neutral
        {
            thesisBuilder.AppendLine("Investment Rationale:");
            thesisBuilder.AppendLine($"The analysis indicates a balanced outlook for {companyName}. ");
            thesisBuilder.AppendLine("Mixed signals suggest a wait-and-see approach may be prudent. ");
            
            if (confidence < 0.60)
                thesisBuilder.AppendLine("Limited data or conflicting indicators reduce conviction in either direction. ");
        }

        return thesisBuilder.ToString().Trim();
    }

    /// <summary>
    /// Generates insights from score components and data.
    /// </summary>
    public static List<Insight> GenerateInsights(
        ScoreBreakdown scores,
        double sentimentScore,
        int riskCount,
        int highSeverityRiskCount)
    {
        var insights = new List<Insight>();

        // Financial insights
        if (scores.FinancialScore > 75)
        {
            insights.Add(new Insight
            {
                Type = "financial",
                Detail = "Exceptional financial performance with strong revenue growth and profitability metrics.",
                Impact = 0.8
            });
        }
        else if (scores.FinancialScore < 35)
        {
            insights.Add(new Insight
            {
                Type = "financial",
                Detail = "Weak financial fundamentals raise concerns about long-term sustainability.",
                Impact = -0.7
            });
        }

        // Sentiment insights
        if (sentimentScore > 0.5)
        {
            insights.Add(new Insight
            {
                Type = "sentiment",
                Detail = "Strong positive sentiment in recent news suggests favorable market perception.",
                Impact = 0.6
            });
        }
        else if (sentimentScore < -0.3)
        {
            insights.Add(new Insight
            {
                Type = "sentiment",
                Detail = "Negative news sentiment indicates market concerns about near-term prospects.",
                Impact = -0.5
            });
        }

        // Risk insights
        if (highSeverityRiskCount >= 3)
        {
            insights.Add(new Insight
            {
                Type = "risk",
                Detail = $"Multiple high-severity risks identified ({highSeverityRiskCount} critical factors) require careful monitoring.",
                Impact = -0.7
            });
        }
        else if (riskCount > 5)
        {
            insights.Add(new Insight
            {
                Type = "risk",
                Detail = $"Elevated risk count ({riskCount} factors) suggests complex risk profile.",
                Impact = -0.4
            });
        }
        else if (riskCount < 2)
        {
            insights.Add(new Insight
            {
                Type = "risk",
                Detail = "Limited identified risks may indicate stable operating environment or incomplete risk assessment.",
                Impact = 0.3
            });
        }

        // Valuation insight based on composite
        if (scores.CompositeScore > 80)
        {
            insights.Add(new Insight
            {
                Type = "valuation",
                Detail = "High composite score suggests strong investment case, but consider current valuation multiples.",
                Impact = 0.5
            });
        }

        return insights;
    }
}
