using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using VantageFlow.Core.Modules.TaskManager.Data;
using VantageFlow.Core.Modules.TaskManager.Models;
using VantageFlow.Core.Modules.TaskManager.Services;

namespace VantageFlow.Tests.Modules.TaskManager;

/// <summary>
/// Tier 2 (Documentation/02-architecture-and-testing-strategy.md §11): the real SQLite-backed
/// repositories against a real, throwaway database file — no fake, since a temp SQLite file
/// is fast, deterministic, and IS the real behavior, unlike the OS Task Scheduler the reference
/// app had to fake out.
/// </summary>
public sealed class TaskRepositoryTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"vantageflow-test-{Guid.NewGuid()}.db");
    private readonly IDbContextFactory<TaskManagerDbContext> _contextFactory;

    public TaskRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
            .UseSqlite($"Data Source={_dbPath}")
            .Options;
        _contextFactory = new PooledDbContextFactory<TaskManagerDbContext>(options);

        using var db = _contextFactory.CreateDbContext();
        db.Database.Migrate();
    }

    public void Dispose()
    {
        // Microsoft.Data.Sqlite pools connections by connection string, keeping the file handle
        // open even after every DbContext using it has been disposed — without this, deleting
        // the temp file fails with "being used by another process".
        SqliteConnection.ClearAllPools();

        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }

    [Fact]
    public async Task AddAsync_PersistsTaskWithAnAlreadySavedRequester_WithoutDuplicatingThePerson()
    {
        // Regression guard for the "disconnected entity" gotcha in TaskRepository.AddAsync:
        // Requester was saved via one DbContext instance (PersonRepository) and is then used
        // on a Task saved via a *different* instance (TaskRepository) — exactly what happens
        // in the real app, since every repository call gets a fresh DbContext from the factory.
        var people = new PersonRepository(_contextFactory);
        var tasks = new TaskRepository(_contextFactory);

        var person = new Person { Name = "Sarah", Relationship = "Manager" };
        await people.AddAsync(person);

        await tasks.AddAsync(new TaskItem { Title = "Pull together the report", Requester = person });

        var savedTasks = await tasks.GetAllAsync();
        var savedPeople = await people.GetAllAsync();

        Assert.Equal("Sarah", Assert.Single(savedTasks).Requester?.Name);
        Assert.Single(savedPeople); // not duplicated by the Task save
    }

    [Fact]
    public async Task AddAsync_PersistsATaskWithNoRequester()
    {
        var tasks = new TaskRepository(_contextFactory);

        await tasks.AddAsync(new TaskItem { Title = "Clean the garage" });

        var savedTasks = await tasks.GetAllAsync();

        Assert.Null(Assert.Single(savedTasks).Requester);
    }
}
