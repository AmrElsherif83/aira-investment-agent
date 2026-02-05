namespace Aira.Core.Interfaces;

using Aira.Core.Models;

/// <summary>
/// Multi-step agentic orchestrator that executes the investment analysis workflow.
/// Coordinates planning, data gathering, synthesis, and reflection steps.
/// </summary>
public interface IAnalysisAgent
{
    /// <summary>
    /// Executes the complete multi-step analysis workflow for a ticker.
    /// </summary>
    /// <param name="ticker">Stock ticker symbol to analyze.</param>
    /// <param name="cancellationToken">Cancellation token for graceful shutdown.</param>
    /// <returns>
    /// A tuple containing the final investment report and the list of step results
    /// showing the agent's execution trace.
    /// </returns>
    Task<(InvestmentReport Report, List<AgentStepResult> Steps)> ExecuteAsync(
        string ticker,
        CancellationToken cancellationToken);
}
