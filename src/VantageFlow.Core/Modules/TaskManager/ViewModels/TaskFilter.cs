namespace VantageFlow.Core.Modules.TaskManager.ViewModels;

/// <summary>
/// How the task list is currently filtered — a presentation concern (how the user chooses to
/// view their list right now), not a domain concept, which is why it lives here rather than in
/// Models alongside TaskState.
/// </summary>
public enum TaskFilter
{
    /// <summary>Everything except Completed — the default view.</summary>
    Active,

    NotStarted,
    InProgress,
    Completed,

    /// <summary>No filtering at all, including Completed.</summary>
    All,
}
