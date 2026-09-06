using VantageFlow.Core.Modules.Notes.Models;
using VantageFlow.Core.Modules.Notes.Services;

namespace VantageFlow.Tests.Modules.Notes.Fakes;

/// <summary>In-memory stand-in for the real (SQLite-backed) repository — a true external
/// boundary, per Documentation/02-architecture-and-testing-strategy.md §11 Tier 1.</summary>
public sealed class FakeNoteRepository : INoteRepository
{
    private readonly List<Note> _notes = [];
    private int _nextId = 1;

    public Task<IReadOnlyList<Note>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<Note>>(_notes.ToList());

    public Task AddAsync(Note note)
    {
        note.Id = _nextId++;
        _notes.Add(note);
        return Task.CompletedTask;
    }
}
