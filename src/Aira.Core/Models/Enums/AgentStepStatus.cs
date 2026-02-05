namespace Aira.Core.Models.Enums;

/// <summary>
/// Represents the execution status of an individual agent step.
/// </summary>
public enum AgentStepStatus
{
    /// <summary>
    /// Step has not started yet.
    /// </summary>
    NotStarted,

    /// <summary>
    /// Step is currently executing.
    /// </summary>
    Running,

    /// <summary>
    /// Step completed successfully.
    /// </summary>
    Succeeded,

    /// <summary>
    /// Step failed with an error.
    /// </summary>
    Failed
}
