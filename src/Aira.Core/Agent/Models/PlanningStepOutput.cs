namespace Aira.Core.Agent.Models;

/// <summary>
/// Structured output from the planning step.
/// </summary>
public class PlanningStepOutput
{
    /// <summary>
    /// The company ticker being analyzed.
    /// </summary>
    public required string Ticker { get; set; }

    /// <summary>
    /// List of planned analysis steps.
    /// </summary>
    public List<string> PlannedSteps { get; set; } = new();

    /// <summary>
    /// List of data sources/tools required for analysis.
    /// </summary>
    public List<string> RequiredTools { get; set; } = new();

    /// <summary>
    /// Key metrics and information to gather.
    /// </summary>
    public List<string> KeyMetrics { get; set; } = new();

    /// <summary>
    /// Expected analysis focus areas.
    /// </summary>
    public List<string> FocusAreas { get; set; } = new();
}
