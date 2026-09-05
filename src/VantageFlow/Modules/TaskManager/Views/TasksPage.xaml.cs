using Microsoft.UI.Xaml.Controls;
using VantageFlow.Core.Modules.TaskManager.ViewModels;

namespace VantageFlow.Modules.TaskManager.Views;

public sealed partial class TasksPage : Page
{
    public TasksViewModel ViewModel { get; }

    public TasksPage()
    {
        ViewModel = App.GetService<TasksViewModel>();
        InitializeComponent();
    }
}
