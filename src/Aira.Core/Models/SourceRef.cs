namespace Aira.Core.Models;

/// <summary>
/// Represents a reference to a data source used in the analysis.
/// </summary>
public class SourceRef
{
    /// <summary>
    /// Type of the source (e.g., "financials", "news", "market", "mock").
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Reference URI or identifier for the source (e.g., "mock://financials/NVDA").
    /// </summary>
    public required string Ref { get; set; }

    /// <summary>
    /// Optional notes or metadata about the source.
    /// </summary>
    public string? Notes { get; set; }
}
