namespace Aira.Core.Agent.Models;

using Aira.Core.Models.DTOs;

/// <summary>
/// Structured output from the data gathering step.
/// </summary>
public class GatheringStepOutput
{
    /// <summary>
    /// Financial snapshot data.
    /// </summary>
    public FinancialSnapshot? FinancialData { get; set; }

    /// <summary>
    /// List of news items gathered.
    /// </summary>
    public List<NewsItem> NewsItems { get; set; } = new();

    /// <summary>
    /// List of identified risk factors.
    /// </summary>
    public List<RiskItem> Risks { get; set; } = new();

    /// <summary>
    /// Data sources accessed during gathering.
    /// </summary>
    public List<string> SourcesAccessed { get; set; } = new();

    /// <summary>
    /// Warnings about missing or incomplete data.
    /// </summary>
    public List<string> DataWarnings { get; set; } = new();

    /// <summary>
    /// Overall data completeness assessment (0.0 to 1.0).
    /// </summary>
    public double DataCompleteness { get; set; }
}
