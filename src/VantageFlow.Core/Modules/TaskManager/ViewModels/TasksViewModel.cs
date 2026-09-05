using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VantageFlow.Core.Modules.TaskManager.Models;
using VantageFlow.Core.Modules.TaskManager.Services;

namespace VantageFlow.Core.Modules.TaskManager.ViewModels;

public sealed partial class TasksViewModel(ITaskRepository taskRepository, IPersonRepository personRepository)
    : ObservableObject
{
    public ObservableCollection<TaskItem> Tasks { get; } = [];

    /// <summary>
    /// The reusable Person list Requester/Recipient are picked from — see CONTEXT.md's Person
    /// entry for why this is a shared list rather than free text per task.
    /// </summary>
    public ObservableCollection<Person> People { get; } = [];

    [RelayCommand]
    public async Task LoadAsync()
    {
        Tasks.Clear();
        foreach (var task in await taskRepository.GetAllAsync())
        {
            Tasks.Add(task);
        }

        People.Clear();
        foreach (var person in await personRepository.GetAllAsync())
        {
            People.Add(person);
        }
    }

    public async Task AddTaskAsync(TaskItem task)
    {
        await taskRepository.AddAsync(task);
        Tasks.Add(task);
    }

    public async Task AddPersonAsync(Person person)
    {
        await personRepository.AddAsync(person);
        People.Add(person);
    }
}
