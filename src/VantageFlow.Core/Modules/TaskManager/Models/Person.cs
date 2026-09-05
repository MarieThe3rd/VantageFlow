namespace VantageFlow.Core.Modules.TaskManager.Models;

/// <summary>
/// A reusable individual — referenced by Tasks as a Requester or Recipient. See CONTEXT.md.
/// </summary>
public sealed class Person
{
    public required string Name { get; set; }

    /// <summary>E.g. "Manager", "Coworker", "Client".</summary>
    public string? Relationship { get; set; }
}
