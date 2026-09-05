namespace VantageFlow.Core.Modules.TaskManager.Models;

/// <summary>
/// A unit of work tracked from creation to completion. Requester, Recipient, Source, and
/// Project are independent, all-optional facts — see CONTEXT.md for why none of them imply
/// or exclude the others.
/// </summary>
public sealed class TaskItem
{
    /// <summary>0 until persisted; EF Core assigns the real value on save.</summary>
    public int Id { get; set; }

    // Not `required` — see the same note on Person.Name; enforced in AddTaskDialog instead.
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public Commitment Commitment { get; set; } = Commitment.Obligation;
    public bool IsComplete { get; set; }

    public Person? Requester { get; set; }
    public Person? Recipient { get; set; }
    public Project? Project { get; set; }

    public Source? Source { get; set; }
    public string? TicketNumber { get; set; }
    public string? TicketLink { get; set; }
}
