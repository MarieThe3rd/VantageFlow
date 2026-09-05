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

    public AddTaskDialog(IEnumerable<Person> people)
    {
        InitializeComponent();
        RequesterCombo.ItemsSource = people.ToList();
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
        Result = new TaskItem
        {
            Title = title,
            Notes = notes.Length > 0 ? notes : null,
            Commitment = IdeaCheckBox.IsChecked == true ? Commitment.Idea : Commitment.Obligation,
            Requester = RequesterCombo.SelectedItem as Person,
        };
    }
}
