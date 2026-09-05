namespace VantageFlow.Core.Modules.TaskManager.Models;

/// <summary>
/// Whether a Task is something the user has to do, or just an idea they had. See CONTEXT.md.
/// </summary>
public enum Commitment
{
    Obligation,
    Idea,
}
