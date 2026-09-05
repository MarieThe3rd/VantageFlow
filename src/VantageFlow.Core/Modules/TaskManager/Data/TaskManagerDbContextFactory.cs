using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VantageFlow.Core.Modules.TaskManager.Data;

/// <summary>
/// Design-time only — lets `dotnet ef migrations add` create a DbContext instance without
/// booting the WinUI app (whose startup is platform-specific and lives in a different project).
/// Never used at runtime; the real app supplies its own connection string via DI.
/// </summary>
public sealed class TaskManagerDbContextFactory : IDesignTimeDbContextFactory<TaskManagerDbContext>
{
    public TaskManagerDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
            .UseSqlite("Data Source=design-time.db")
            .Options;

        return new TaskManagerDbContext(options);
    }
}
