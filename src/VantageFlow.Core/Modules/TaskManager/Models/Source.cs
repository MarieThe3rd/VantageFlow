namespace VantageFlow.Core.Modules.TaskManager.Models;

/// <summary>
/// A medium a request can arrive through — Email, Meeting, or a specific ticketing system
/// (e.g. "Ivanti Ticket", "ADO Work Item"). User-maintainable, like Person: a new ticketing
/// system is a new entry, not a code change. See CONTEXT.md.
/// </summary>
public sealed class Source
{
    /// <summary>0 until persisted; EF Core assigns the real value on save.</summary>
    public int Id { get; set; }

    // Not `required` — see Person.Name's note; enforced wherever a Source is actually created.
    public string Name { get; set; } = string.Empty;

    /// <summary>Whether Tasks using this Source carry a Ticket Number and Ticket Link.</summary>
    public bool IsTicket { get; set; }
}
