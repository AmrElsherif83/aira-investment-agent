namespace Aira.Core.Agent.Models;

using Aira.Core.Models;

/// <summary>
/// Structured output from the scoring and synthesis step.
/// </summary>
public class ScoringStepOutput
{
    /// <summary>
    /// Detailed score breakdown with component scores and weights.
    /// </summary>
    public required ScoreBreakdown ScoreBreakdown { get; set; }

    /// <summary>
    /// Preliminary investment signal before reflection.
    /// </summary>
    public required string PreliminarySignal { get; set; }

    /// <summary>
    /// Preliminary confidence score (0.0 to 1.0).
    /// </summary>
    public double PreliminaryConfidence { get; set; }

    /// <summary>
    /// Generated investment thesis.
    /// </summary>
    public string? Thesis { get; set; }

    /// <summary>
    /// List of insights generated during synthesis.
    /// </summary>
    public List<Insight> Insights { get; set; } = new();

    /// <summary>
    /// Intermediate scoring calculations for explainability.
    /// </summary>
    public Dictionary<string, object> ScoringDetails { get; set; } = new();
}
