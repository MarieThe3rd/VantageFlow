using Microsoft.EntityFrameworkCore;
using VantageFlow.Core.Modules.TaskManager.Data;
using VantageFlow.Core.Modules.TaskManager.Models;

namespace VantageFlow.Core.Modules.TaskManager.Services;

public sealed class SourceRepository(IDbContextFactory<TaskManagerDbContext> contextFactory) : ISourceRepository
{
    public async Task<IReadOnlyList<Source>> GetAllAsync()
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.Sources.ToListAsync();
    }

    public async Task AddAsync(Source source)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        db.Sources.Add(source);
        await db.SaveChangesAsync();
    }
}
