using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VantageFlow.Core.Modules.TaskManager.Models;
using VantageFlow.Core.Modules.TaskManager.Services;

namespace VantageFlow.Core.Modules.TaskManager.ViewModels;

public sealed partial class TasksViewModel(
    ITaskRepository taskRepository,
    IPersonRepository personRepository,
    IProjectRepository projectRepository,
    ISourceRepository sourceRepository)
    : ObservableObject
{
    public ObservableCollection<TaskItem> Tasks { get; } = [];

    /// <summary>
    /// Reusable lists Requester/Recipient, Project, and Source are picked from — see CONTEXT.md
    /// for why these are shared lists rather than free text per task.
    /// </summary>
    public ObservableCollection<Person> People { get; } = [];

    public ObservableCollection<Project> Projects { get; } = [];

    public ObservableCollection<Source> Sources { get; } = [];

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

        Projects.Clear();
        foreach (var project in await projectRepository.GetAllAsync())
        {
            Projects.Add(project);
        }

        Sources.Clear();
        foreach (var source in await sourceRepository.GetAllAsync())
        {
            Sources.Add(source);
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

    public async Task AddProjectAsync(Project project)
    {
        await projectRepository.AddAsync(project);
        Projects.Add(project);
    }

    public async Task AddSourceAsync(Source source)
    {
        await sourceRepository.AddAsync(source);
        Sources.Add(source);
    }
}
