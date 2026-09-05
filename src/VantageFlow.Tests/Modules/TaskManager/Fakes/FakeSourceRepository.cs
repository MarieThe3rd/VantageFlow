using VantageFlow.Core.Modules.TaskManager.Models;
using VantageFlow.Core.Modules.TaskManager.Services;

namespace VantageFlow.Tests.Modules.TaskManager.Fakes;

/// <summary>In-memory stand-in for the real (SQLite-backed) repository — a true external
/// boundary, per Documentation/02-architecture-and-testing-strategy.md §11 Tier 1.</summary>
public sealed class FakeSourceRepository : ISourceRepository
{
    private readonly List<Source> _sources = [];
    private int _nextId = 1;

    public Task<IReadOnlyList<Source>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<Source>>(_sources.ToList());

    public Task AddAsync(Source source)
    {
        source.Id = _nextId++;
        _sources.Add(source);
        return Task.CompletedTask;
    }
}
