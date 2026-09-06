using Microsoft.EntityFrameworkCore;
using VantageFlow.Core.Modules.Notes.Data;
using VantageFlow.Core.Modules.Notes.Models;

namespace VantageFlow.Core.Modules.Notes.Services;

public sealed class NoteRepository(IDbContextFactory<NotesDbContext> contextFactory) : INoteRepository
{
    public async Task<IReadOnlyList<Note>> GetAllAsync()
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.Notes.ToListAsync();
    }

    public async Task AddAsync(Note note)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        db.Notes.Add(note);
        await db.SaveChangesAsync();
    }
}
