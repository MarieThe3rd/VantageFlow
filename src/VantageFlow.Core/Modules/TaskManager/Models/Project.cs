namespace VantageFlow.Core.Modules.TaskManager.Models;

/// <summary>
/// A tracked effort containing the Tasks that belong to it. See CONTEXT.md.
/// </summary>
public sealed class Project
{
    /// <summary>0 until persisted; EF Core assigns the real value on save.</summary>
    public int Id { get; set; }

    // Not `required` — see Person.Name's note; enforced wherever a Project is actually created.
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly? TargetDate { get; set; }
}
