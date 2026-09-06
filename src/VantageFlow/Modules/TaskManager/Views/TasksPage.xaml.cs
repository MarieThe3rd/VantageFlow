using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using VantageFlow.Core.Modules.TaskManager.Models;
using VantageFlow.Core.Modules.TaskManager.ViewModels;
using VantageFlow.Modules.TaskManager.Views.Dialogs;

namespace VantageFlow.Modules.TaskManager.Views;

public sealed partial class TasksPage : Page
{
    public TasksViewModel ViewModel { get; }

    public TaskFilter[] FilterOptions { get; } = Enum.GetValues<TaskFilter>();

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
        var dialog = new AddTaskDialog(ViewModel.People, ViewModel.Projects, ViewModel.Sources) { XamlRoot = XamlRoot };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary && dialog.Result is not null)
        {
            await ViewModel.AddTaskAsync(dialog.Result);
        }
    }

    private async void EditTask_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not TaskItem existing)
        {
            return;
        }

        var dialog = new AddTaskDialog(ViewModel.People, ViewModel.Projects, ViewModel.Sources, existing) { XamlRoot = XamlRoot };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary && dialog.Result is not null)
        {
            await ViewModel.UpdateTaskAsync(dialog.Result);
        }
    }

    private void TaskStarted_Changed(object sender, RoutedEventArgs e) => DeferUpdate(sender);

    private void TaskComplete_Changed(object sender, RoutedEventArgs e) => DeferUpdate(sender);

    // Toggling Started/Done can change which filtered bucket a task belongs to, which means
    // FilteredTasks — the ListView's own bound collection — may need an item added or removed.
    // The CheckBox raising this event lives inside that same ListView's item template, so doing
    // that synchronously risks the exact crash fixed in Documentation/Walkthroughs/10 ("Child
    // collection must not be modified during measure or arrange"). Unlike that fix, there's no
    // way to avoid a real collection change here — an item genuinely needs to appear or
    // disappear — so the correct tool this time is deferring the mutation to the next UI dispatch
    // cycle, once the current layout pass has fully settled.
    private void DeferUpdate(object sender)
    {
        if (((FrameworkElement)sender).DataContext is not TaskItem task)
        {
            return;
        }

        DispatcherQueue.TryEnqueue(async () => await ViewModel.UpdateTaskAsync(task));
    }

    private async void NewPerson_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddPersonDialog { XamlRoot = XamlRoot };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary && dialog.Result is not null)
        {
            await ViewModel.AddPersonAsync(dialog.Result);
        }
    }

    private async void NewProject_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddProjectDialog { XamlRoot = XamlRoot };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary && dialog.Result is not null)
        {
            await ViewModel.AddProjectAsync(dialog.Result);
        }
    }

    private async void NewSource_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddSourceDialog { XamlRoot = XamlRoot };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary && dialog.Result is not null)
        {
            await ViewModel.AddSourceAsync(dialog.Result);
        }
    }
}
