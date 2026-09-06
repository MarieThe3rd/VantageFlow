namespace VantageFlow.Core.Modules.TaskManager.Models;

/// <summary>
/// Where a Task stands — always derived from StartDate/CompletedDate (see TaskItem.State),
/// never its own stored fact, so it can't drift out of sync with them. Named "TaskState," not
/// "TaskStatus," to avoid colliding with the unrelated System.Threading.Tasks.TaskStatus already
/// in scope via the project's implicit usings.
/// </summary>
public enum TaskState
{
    NotStarted,
    InProgress,
    Completed,
}
