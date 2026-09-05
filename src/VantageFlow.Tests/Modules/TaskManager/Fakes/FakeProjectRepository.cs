using VantageFlow.Core.Modules.TaskManager.Models;
using VantageFlow.Core.Modules.TaskManager.Services;

namespace VantageFlow.Tests.Modules.TaskManager.Fakes;

/// <summary>In-memory stand-in for the real (SQLite-backed) repository — a true external
/// boundary, per Documentation/02-architecture-and-testing-strategy.md §11 Tier 1.</summary>
public sealed class FakeProjectRepository : IProjectRepository
{
    private readonly List<Project> _projects = [];
    private int _nextId = 1;

    public Task<IReadOnlyList<Project>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<Project>>(_projects.ToList());

    public Task AddAsync(Project project)
    {
        project.Id = _nextId++;
        _projects.Add(project);
        return Task.CompletedTask;
    }
}
