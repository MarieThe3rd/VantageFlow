using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VantageFlow.Core.Modules.TaskManager.Models;

namespace VantageFlow.Modules.TaskManager.Views.Dialogs;

/// <summary>
/// Constructor-in, Result-property-out — the one dialog shape used everywhere
/// (see Documentation/02-architecture-and-testing-strategy.md §6).
/// </summary>
public sealed partial class AddTaskDialog : ContentDialog
{
    public TaskItem? Result { get; private set; }

    public AddTaskDialog(IEnumerable<Person> people, IEnumerable<Project> projects, IEnumerable<Source> sources)
    {
        InitializeComponent();

        var peopleList = people.ToList();
        RequesterCombo.ItemsSource = peopleList;
        RecipientCombo.ItemsSource = peopleList;
        ProjectCombo.ItemsSource = projects.ToList();
        SourceCombo.ItemsSource = sources.ToList();
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

        Result = new TaskItem
        {
            Title = title,
            Notes = notes.Length > 0 ? notes : null,
            Commitment = IdeaCheckBox.IsChecked == true ? Commitment.Idea : Commitment.Obligation,
            Requester = RequesterCombo.SelectedItem as Person,
            Recipient = RecipientCombo.SelectedItem as Person,
            Project = ProjectCombo.SelectedItem as Project,
            Source = source,
            TicketNumber = source is { IsTicket: true } && ticketNumber.Length > 0 ? ticketNumber : null,
            TicketLink = source is { IsTicket: true } && ticketLink.Length > 0 ? ticketLink : null,
        };
    }
}
