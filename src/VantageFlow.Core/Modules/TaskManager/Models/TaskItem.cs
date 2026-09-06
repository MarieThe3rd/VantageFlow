using CommunityToolkit.Mvvm.ComponentModel;

namespace VantageFlow.Core.Modules.TaskManager.Models;

/// <summary>
/// A unit of work tracked from creation to completion. Requester, Recipient, Source, and
/// Project are independent, all-optional facts — see CONTEXT.md for why none of them imply
/// or exclude the others.
///
/// Observable (not plain data, unlike Person/Project/Source) because it's edited in place while
/// already displayed in a bound ListView — the complete/incomplete checkbox and Edit dialog both
/// mutate an existing instance rather than replacing it. Without PropertyChanged notifications,
/// the only way to refresh the UI was replacing the item in its ObservableCollection, which
/// crashed ("Child collection must not be modified during measure or arrange") when triggered
/// from a control that is itself part of that same list's item template.
/// </summary>
public sealed partial class TaskItem : ObservableObject
{
    /// <summary>0 until persisted; EF Core assigns the real value on save.</summary>
    public int Id { get; set; }

    // Not `required` — see the same note on Person.Name; enforced in AddTaskDialog instead.
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string? _notes;

    [ObservableProperty]
    private Commitment _commitment = Commitment.Obligation;

    [ObservableProperty]
    private bool _isComplete;

    [ObservableProperty]
    private Person? _requester;

    [ObservableProperty]
    private Person? _recipient;

    [ObservableProperty]
    private Project? _project;

    [ObservableProperty]
    private Source? _source;

    [ObservableProperty]
    private string? _ticketNumber;

    [ObservableProperty]
    private string? _ticketLink;
}
