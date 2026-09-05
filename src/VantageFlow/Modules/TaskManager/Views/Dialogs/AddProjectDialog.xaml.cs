using Microsoft.UI.Xaml.Controls;
using VantageFlow.Core.Modules.TaskManager.Models;

namespace VantageFlow.Modules.TaskManager.Views.Dialogs;

/// <summary>
/// Constructor-in, Result-property-out — the one dialog shape used everywhere
/// (see Documentation/02-architecture-and-testing-strategy.md §6).
/// </summary>
public sealed partial class AddProjectDialog : ContentDialog
{
    public Project? Result { get; private set; }

    public AddProjectDialog()
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

        var description = DescriptionBox.Text.Trim();
        Result = new Project
        {
            Name = name,
            Description = description.Length > 0 ? description : null,
            TargetDate = TargetDatePicker.Date is { } date ? DateOnly.FromDateTime(date.Date) : null,
        };
    }
}
