namespace Aira.Core.Models;

/// <summary>
/// The final structured investment analysis report containing thesis, signal, insights, and supporting data.
/// </summary>
public class InvestmentReport
{
    /// <summary>
    /// Company name (e.g., "NVIDIA").
    /// </summary>
    public required string Company { get; set; }

    /// <summary>
    /// Investment thesis summarizing the analysis and recommendation rationale.
    /// </summary>
    public required string Thesis { get; set; }

    /// <summary>
    /// Investment signal: "Bullish", "Neutral", or "Bearish".
    /// </summary>
    public required string Signal { get; set; }

    /// <summary>
    /// List of insights generated during the analysis.
    /// </summary>
    public required List<Insight> Insights { get; set; }

    /// <summary>
    /// List of data sources referenced in the analysis.
    /// </summary>
    public required List<SourceRef> Sources { get; set; }

    /// <summary>
    /// Confidence level in the recommendation (0.0 to 1.0).
    /// Higher values indicate greater confidence.
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Known limitations or caveats in the analysis (e.g., missing data, contradictions).
    /// </summary>
    public List<string>? Limitations { get; set; }

    /// <summary>
    /// Detailed breakdown of how scores were calculated for explainability.
    /// </summary>
    public ScoreBreakdown? ScoreBreakdown { get; set; }

    /// <summary>
    /// Timestamp when the report was generated.
    /// </summary>
    public DateTimeOffset GeneratedAt { get; set; }
}
