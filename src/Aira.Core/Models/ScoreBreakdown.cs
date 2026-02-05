namespace Aira.Core.Models;

/// <summary>
/// Provides an explainable breakdown of how the investment signal was calculated.
/// </summary>
public class ScoreBreakdown
{
    /// <summary>
    /// Score derived from financial metrics (e.g., revenue, profitability, margins).
    /// </summary>
    public double FinancialScore { get; set; }

    /// <summary>
    /// Score derived from news sentiment and public perception.
    /// </summary>
    public double SentimentScore { get; set; }

    /// <summary>
    /// Score derived from market metrics (e.g., price trends, volume, valuation ratios).
    /// </summary>
    public double MarketScore { get; set; }

    /// <summary>
    /// Overall composite score calculated from weighted components.
    /// </summary>
    public double CompositeScore { get; set; }

    /// <summary>
    /// Weight applied to financial score in composite calculation.
    /// </summary>
    public double FinancialWeight { get; set; }

    /// <summary>
    /// Weight applied to sentiment score in composite calculation.
    /// </summary>
    public double SentimentWeight { get; set; }

    /// <summary>
    /// Weight applied to market score in composite calculation.
    /// </summary>
    public double MarketWeight { get; set; }
}
