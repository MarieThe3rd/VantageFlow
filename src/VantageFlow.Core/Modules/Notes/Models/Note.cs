namespace VantageFlow.Core.Modules.Notes.Models;

/// <summary>A freeform note — the second module, deliberately simple: no reusable entities of
/// its own, proving IAppModule generalizes without needing TaskManager's richer shape.</summary>
public sealed class Note
{
    /// <summary>0 until persisted; EF Core assigns the real value on save.</summary>
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
