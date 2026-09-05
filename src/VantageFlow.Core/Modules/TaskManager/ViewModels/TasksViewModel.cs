using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using VantageFlow.Core.Modules.TaskManager.Models;

namespace VantageFlow.Core.Modules.TaskManager.ViewModels;

public sealed partial class TasksViewModel : ObservableObject
{
    public ObservableCollection<TaskItem> Tasks { get; } = [];

    /// <summary>
    /// The reusable Person list Requester/Recipient are picked from — see CONTEXT.md's Person
    /// entry for why this is a shared list rather than free text per task.
    /// </summary>
    public ObservableCollection<Person> People { get; } = [];

    public void AddTask(TaskItem task) => Tasks.Add(task);

    public void AddPerson(Person person) => People.Add(person);
}
