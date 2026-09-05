using Microsoft.UI.Xaml.Controls;
using VantageFlow.Core.Modules.TaskManager.Models;

namespace VantageFlow.Modules.TaskManager.Views.Dialogs;

/// <summary>
/// Constructor-in, Result-property-out — the one dialog shape used everywhere
/// (see Documentation/02-architecture-and-testing-strategy.md §6).
/// </summary>
public sealed partial class AddPersonDialog : ContentDialog
{
    public Person? Result { get; private set; }

    public AddPersonDialog()
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

        var relationship = RelationshipBox.Text.Trim();
        Result = new Person
        {
            Name = name,
            Relationship = relationship.Length > 0 ? relationship : null,
        };
    }
}
