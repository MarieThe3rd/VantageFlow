namespace VantageFlow.Core.Modules.TaskManager.Models;

/// <summary>
/// A tracked effort containing the Tasks that belong to it. See CONTEXT.md.
/// </summary>
public sealed class Project
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateOnly? TargetDate { get; set; }
}
