using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VantageFlow.Core.Modules.TaskManager.Models;

namespace VantageFlow.Modules.TaskManager.Views.Dialogs;

/// <summary>
/// Constructor-in, Result-property-out — the one dialog shape used everywhere
/// (see Documentation/02-architecture-and-testing-strategy.md §6). Doubles as the edit dialog:
/// pass an existing TaskItem to pre-fill and mutate it in place rather than creating a new one,
/// so the caller gets back the same Id to persist an update against, not a fresh insert.
/// </summary>
public sealed partial class AddTaskDialog : ContentDialog
{
    private readonly TaskItem? _existing;

    public TaskItem? Result { get; private set; }

    public AddTaskDialog(IEnumerable<Person> people, IEnumerable<Project> projects, IEnumerable<Source> sources, TaskItem? existing = null)
    {
        InitializeComponent();
        _existing = existing;

        var peopleList = people.ToList();
        RequesterCombo.ItemsSource = peopleList;
        RecipientCombo.ItemsSource = peopleList;
        ProjectCombo.ItemsSource = projects.ToList();
        SourceCombo.ItemsSource = sources.ToList();

        if (existing is not null)
        {
            Title = "Edit Task";
            PrimaryButtonText = "Save";

            TitleBox.Text = existing.Title;
            NotesBox.Text = existing.Notes ?? string.Empty;
            IdeaCheckBox.IsChecked = existing.Commitment == Commitment.Idea;
            RequesterCombo.SelectedItem = peopleList.FirstOrDefault(p => p.Id == existing.Requester?.Id);
            RecipientCombo.SelectedItem = peopleList.FirstOrDefault(p => p.Id == existing.Recipient?.Id);
            ProjectCombo.SelectedItem = ((List<Project>)ProjectCombo.ItemsSource).FirstOrDefault(p => p.Id == existing.Project?.Id);
            SourceCombo.SelectedItem = ((List<Source>)SourceCombo.ItemsSource).FirstOrDefault(s => s.Id == existing.Source?.Id);
            TicketNumberBox.Text = existing.TicketNumber ?? string.Empty;
            TicketLinkBox.Text = existing.TicketLink ?? string.Empty;
        }
    }

    private void SourceCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Ticket Number/Link are only meaningful for a ticket-type Source — see CONTEXT.md's
        // Source entry. Hidden, not just disabled, so an obligation-only task never shows them.
        var isTicket = SourceCombo.SelectedItem is Source { IsTicket: true };
        TicketFieldsPanel.Visibility = isTicket ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var title = TitleBox.Text.Trim();
        if (title.Length == 0)
        {
            args.Cancel = true;
            return;
        }

        var notes = NotesBox.Text.Trim();
        var source = SourceCombo.SelectedItem as Source;
        var ticketNumber = TicketNumberBox.Text.Trim();
        var ticketLink = TicketLinkBox.Text.Trim();

        var task = _existing ?? new TaskItem();
        task.Title = title;
        task.Notes = notes.Length > 0 ? notes : null;
        task.Commitment = IdeaCheckBox.IsChecked == true ? Commitment.Idea : Commitment.Obligation;
        task.Requester = RequesterCombo.SelectedItem as Person;
        task.Recipient = RecipientCombo.SelectedItem as Person;
        task.Project = ProjectCombo.SelectedItem as Project;
        task.Source = source;
        task.TicketNumber = source is { IsTicket: true } && ticketNumber.Length > 0 ? ticketNumber : null;
        task.TicketLink = source is { IsTicket: true } && ticketLink.Length > 0 ? ticketLink : null;

        Result = task;
    }
}
