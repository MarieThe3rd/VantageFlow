using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using VantageFlow.Core.Modules.Notes.ViewModels;
using VantageFlow.Modules.Notes.Views.Dialogs;

namespace VantageFlow.Modules.Notes.Views;

public sealed partial class NotesPage : Page
{
    public NotesViewModel ViewModel { get; }

    public NotesPage()
    {
        ViewModel = App.GetService<NotesViewModel>();
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.LoadAsync();
    }

    private async void NewNote_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddNoteDialog { XamlRoot = XamlRoot };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary && dialog.Result is not null)
        {
            await ViewModel.AddNoteAsync(dialog.Result);
        }
    }
}
