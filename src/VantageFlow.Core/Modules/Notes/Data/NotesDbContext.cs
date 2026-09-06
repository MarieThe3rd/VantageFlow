using Microsoft.EntityFrameworkCore;
using VantageFlow.Core.Modules.Notes.Models;

namespace VantageFlow.Core.Modules.Notes.Data;

public sealed class NotesDbContext(DbContextOptions<NotesDbContext> options) : DbContext(options)
{
    public DbSet<Note> Notes => Set<Note>();
}
