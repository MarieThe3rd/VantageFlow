using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VantageFlow.Core.Modules.Notes.Data;

/// <summary>
/// Design-time only — lets `dotnet ef migrations add` create a DbContext instance without
/// booting the WinUI app. See TaskManagerDbContextFactory for the same pattern and why it's needed.
/// </summary>
public sealed class NotesDbContextFactory : IDesignTimeDbContextFactory<NotesDbContext>
{
    public NotesDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<NotesDbContext>()
            .UseSqlite("Data Source=design-time.db")
            .Options;

        return new NotesDbContext(options);
    }
}
