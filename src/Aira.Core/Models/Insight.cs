namespace Aira.Core.Models;

/// <summary>
/// Represents an individual insight or finding from the investment analysis.
/// </summary>
public class Insight
{
    /// <summary>
    /// Type or category of the insight (e.g., "financial", "news", "risk", "valuation", "sentiment").
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Detailed description of the insight.
    /// </summary>
    public required string Detail { get; set; }

    /// <summary>
    /// Optional impact score ranging from -1 (negative) to +1 (positive).
    /// Null indicates neutral or unmeasured impact.
    /// </summary>
    public double? Impact { get; set; }
}
