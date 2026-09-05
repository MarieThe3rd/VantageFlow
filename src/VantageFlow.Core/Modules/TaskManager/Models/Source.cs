namespace VantageFlow.Core.Modules.TaskManager.Models;

/// <summary>
/// A medium a request can arrive through — Email, Meeting, or a specific ticketing system
/// (e.g. "Ivanti Ticket", "ADO Work Item"). User-maintainable, like Person: a new ticketing
/// system is a new entry, not a code change. See CONTEXT.md.
/// </summary>
public sealed class Source
{
    public required string Name { get; set; }

    /// <summary>Whether Tasks using this Source carry a Ticket Number and Ticket Link.</summary>
    public bool IsTicket { get; set; }
}
