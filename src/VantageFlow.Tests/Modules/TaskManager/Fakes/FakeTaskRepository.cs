using VantageFlow.Core.Modules.TaskManager.Models;
using VantageFlow.Core.Modules.TaskManager.Services;

namespace VantageFlow.Tests.Modules.TaskManager.Fakes;

/// <summary>In-memory stand-in for the real (SQLite-backed) repository — a true external
/// boundary, per Documentation/02-architecture-and-testing-strategy.md §11 Tier 1.</summary>
public sealed class FakeTaskRepository : ITaskRepository
{
    private readonly List<TaskItem> _tasks = [];

    public Task<IReadOnlyList<TaskItem>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<TaskItem>>(_tasks.ToList());

    public Task AddAsync(TaskItem task)
    {
        _tasks.Add(task);
        return Task.CompletedTask;
    }
}
