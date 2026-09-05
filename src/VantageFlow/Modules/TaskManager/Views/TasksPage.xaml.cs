using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using VantageFlow.Core.Modules.TaskManager.ViewModels;
using VantageFlow.Modules.TaskManager.Views.Dialogs;

namespace VantageFlow.Modules.TaskManager.Views;

public sealed partial class TasksPage : Page
{
    public TasksViewModel ViewModel { get; }

    public TasksPage()
    {
        ViewModel = App.GetService<TasksViewModel>();
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.LoadAsync();
    }

    private async void NewTask_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddTaskDialog(ViewModel.People) { XamlRoot = XamlRoot };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary && dialog.Result is not null)
        {
            await ViewModel.AddTaskAsync(dialog.Result);
        }
    }

    private async void NewPerson_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddPersonDialog { XamlRoot = XamlRoot };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary && dialog.Result is not null)
        {
            await ViewModel.AddPersonAsync(dialog.Result);
        }
    }
}
