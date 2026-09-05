using VantageFlow.Core.Modules.TaskManager.Models;
using VantageFlow.Core.Modules.TaskManager.Services;

namespace VantageFlow.Tests.Modules.TaskManager.Fakes;

/// <summary>In-memory stand-in for the real (SQLite-backed) repository — a true external
/// boundary, per Documentation/02-architecture-and-testing-strategy.md §11 Tier 1.</summary>
public sealed class FakePersonRepository : IPersonRepository
{
    private readonly List<Person> _people = [];
    private int _nextId = 1;

    public Task<IReadOnlyList<Person>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<Person>>(_people.ToList());

    public Task AddAsync(Person person)
    {
        person.Id = _nextId++;
        _people.Add(person);
        return Task.CompletedTask;
    }
}
