namespace VantageFlow.Core.Modules.TaskManager.Models;

/// <summary>
/// A reusable individual — referenced by Tasks as a Requester or Recipient. See CONTEXT.md.
/// </summary>
public sealed class Person
{
    /// <summary>0 until persisted; EF Core assigns the real value on save.</summary>
    public int Id { get; set; }

    // Not `required`: WinUI's generated XAML type metadata (XamlTypeInfo.g.cs) needs a
    // parameterless activator for any type reachable from a binding, and can't satisfy a
    // `required` member. Non-emptiness is enforced where a Person is actually created —
    // see AddPersonDialog's validation — not at the compiler level.
    public string Name { get; set; } = string.Empty;

    /// <summary>E.g. "Manager", "Coworker", "Client".</summary>
    public string? Relationship { get; set; }
}
