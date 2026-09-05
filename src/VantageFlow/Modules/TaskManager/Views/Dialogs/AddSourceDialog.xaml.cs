using Microsoft.UI.Xaml.Controls;
using VantageFlow.Core.Modules.TaskManager.Models;

namespace VantageFlow.Modules.TaskManager.Views.Dialogs;

/// <summary>
/// Constructor-in, Result-property-out — the one dialog shape used everywhere
/// (see Documentation/02-architecture-and-testing-strategy.md §6).
/// </summary>
public sealed partial class AddSourceDialog : ContentDialog
{
    public Source? Result { get; private set; }

    public AddSourceDialog()
    {
        InitializeComponent();
    }

    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var name = NameBox.Text.Trim();
        if (name.Length == 0)
        {
            args.Cancel = true;
            return;
        }

        Result = new Source
        {
            Name = name,
            IsTicket = IsTicketCheckBox.IsChecked == true,
        };
    }
}
