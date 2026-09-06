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
    /// <summary>Every loaded task, unfiltered — what tests and Add/Update reason about.</summary>
    public ObservableCollection<TaskItem> Tasks { get; } = [];

    /// <summary>What the view actually binds to — Tasks narrowed by Filter. Kept as a separate
    /// collection (rather than filtering Tasks itself) so Add/Update always see the full set,
    /// regardless of what's currently visible.</summary>
    public ObservableCollection<TaskItem> FilteredTasks { get; } = [];

    [ObservableProperty]
    private TaskFilter _filter = TaskFilter.Active;

    partial void OnFilterChanged(TaskFilter value) => ApplyFilter();

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

        ApplyFilter();

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
        ApplyFilter();
    }

    public async Task UpdateTaskAsync(TaskItem task)
    {
        await taskRepository.UpdateAsync(task);

        // Property-only changes (Title, Requester, ...) need no collection mutation — TaskItem
        // is observable, so its own PropertyChanged already refreshed the row. But State can
        // change here too (Started/Completed toggled), which can move a task in or out of the
        // *visible* set — a real collection change, which ApplyFilter provides.
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        FilteredTasks.Clear();
        foreach (var task in Tasks)
        {
            if (Matches(task, Filter))
            {
                FilteredTasks.Add(task);
            }
        }
    }

    private static bool Matches(TaskItem task, TaskFilter filter) => filter switch
    {
        TaskFilter.NotStarted => task.State == TaskState.NotStarted,
        TaskFilter.InProgress => task.State == TaskState.InProgress,
        TaskFilter.Completed => task.State == TaskState.Completed,
        TaskFilter.Active => task.State != TaskState.Completed,
        TaskFilter.All => true,
        _ => true,
    };

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
