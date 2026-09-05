using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using VantageFlow.Core.Modules.TaskManager.Models;

namespace VantageFlow.Core.Modules.TaskManager.ViewModels;

public sealed partial class TasksViewModel : ObservableObject
{
    public ObservableCollection<TaskItem> Tasks { get; } = [];
}
