namespace Aira.Core.Models.DTOs;

/// <summary>
/// Represents a known risk factor associated with a company or investment.
/// </summary>
public class RiskItem
{
    /// <summary>
    /// Title or name of the risk factor.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Detailed description of the risk.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Severity level: "low", "medium", "high", or "critical".
    /// </summary>
    public string? Severity { get; set; }

    /// <summary>
    /// Category of risk (e.g., "operational", "market", "regulatory", "technology").
    /// </summary>
    public string? Category { get; set; }
}
