using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using VantageFlow.Core.Modules.Notes.Data;
using VantageFlow.Core.Modules.Notes.Models;
using VantageFlow.Core.Modules.Notes.Services;

namespace VantageFlow.Tests.Modules.Notes;

/// <summary>
/// Tier 2 (Documentation/02-architecture-and-testing-strategy.md §11): the real SQLite-backed
/// repository against a real, throwaway database file. Simpler than TaskRepositoryTests — Note
/// has no relations, so there's no disconnected-entity concern to guard against here.
/// </summary>
public sealed class NoteRepositoryTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"vantageflow-notes-test-{Guid.NewGuid()}.db");
    private readonly IDbContextFactory<NotesDbContext> _contextFactory;

    public NoteRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<NotesDbContext>()
            .UseSqlite($"Data Source={_dbPath}")
            .Options;
        _contextFactory = new PooledDbContextFactory<NotesDbContext>(options);

        using var db = _contextFactory.CreateDbContext();
        db.Database.Migrate();
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();

        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }

    [Fact]
    public async Task AddAsync_ThenGetAllAsync_RoundTripsANote()
    {
        var repository = new NoteRepository(_contextFactory);

        await repository.AddAsync(new Note { Title = "Idea", Body = "Try the thing" });

        var saved = Assert.Single(await repository.GetAllAsync());
        Assert.Equal("Idea", saved.Title);
        Assert.Equal("Try the thing", saved.Body);
    }
}
