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

    [Fact]
    public async Task UpdateAsync_PersistsChanges_WithoutDuplicatingAnAlreadySavedRequester()
    {
        // Same disconnected-entity concern as AddAsync, but for Update's graph walk: the
        // Requester attached here must stay Unchanged, not get marked Modified or re-inserted.
        var people = new PersonRepository(_contextFactory);
        var tasks = new TaskRepository(_contextFactory);

        var person = new Person { Name = "Sarah" };
        await people.AddAsync(person);

        var task = new TaskItem { Title = "Draft the proposal" };
        await tasks.AddAsync(task);

        task.IsComplete = true;
        task.Requester = person;
        await tasks.UpdateAsync(task);

        var savedTasks = await tasks.GetAllAsync();
        var savedPeople = await people.GetAllAsync();
        var savedTask = Assert.Single(savedTasks);

        Assert.True(savedTask.IsComplete);
        Assert.Equal("Sarah", savedTask.Requester?.Name);
        Assert.Single(savedPeople); // not duplicated by the update
    }

    [Fact]
    public async Task AddAsync_ThenUpdateAsync_RoundTripsStartDueAndCompletedDates()
    {
        var tasks = new TaskRepository(_contextFactory);
        var startDate = new DateOnly(2026, 1, 5);
        var dueDate = new DateOnly(2026, 1, 10);

        var task = new TaskItem { Title = "File the report", StartDate = startDate, DueDate = dueDate };
        await tasks.AddAsync(task);

        var afterAdd = Assert.Single(await tasks.GetAllAsync());
        Assert.Equal(startDate, afterAdd.StartDate);
        Assert.Equal(dueDate, afterAdd.DueDate);
        Assert.Null(afterAdd.CompletedDate);

        task.IsComplete = true;
        await tasks.UpdateAsync(task);

        var afterUpdate = Assert.Single(await tasks.GetAllAsync());
        Assert.Equal(DateOnly.FromDateTime(DateTime.Today), afterUpdate.CompletedDate);
        Assert.True(afterUpdate.IsComplete);
    }
}
