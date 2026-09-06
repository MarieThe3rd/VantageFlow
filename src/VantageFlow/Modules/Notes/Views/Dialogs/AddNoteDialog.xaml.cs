using Microsoft.UI.Xaml.Controls;
using VantageFlow.Core.Modules.Notes.Models;

namespace VantageFlow.Modules.Notes.Views.Dialogs;

/// <summary>
/// Constructor-in, Result-property-out — the one dialog shape used everywhere
/// (see Documentation/02-architecture-and-testing-strategy.md §6).
/// </summary>
public sealed partial class AddNoteDialog : ContentDialog
{
    public Note? Result { get; private set; }

    public AddNoteDialog()
    {
        InitializeComponent();
    }

    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var title = TitleBox.Text.Trim();
        if (title.Length == 0)
        {
            args.Cancel = true;
            return;
        }

        Result = new Note { Title = title, Body = BodyBox.Text.Trim() };
    }
}
