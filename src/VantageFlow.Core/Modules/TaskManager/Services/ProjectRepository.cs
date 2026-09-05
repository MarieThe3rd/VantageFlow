using Microsoft.EntityFrameworkCore;
using VantageFlow.Core.Modules.TaskManager.Data;
using VantageFlow.Core.Modules.TaskManager.Models;

namespace VantageFlow.Core.Modules.TaskManager.Services;

public sealed class ProjectRepository(IDbContextFactory<TaskManagerDbContext> contextFactory) : IProjectRepository
{
    public async Task<IReadOnlyList<Project>> GetAllAsync()
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.Projects.ToListAsync();
    }

    public async Task AddAsync(Project project)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        db.Projects.Add(project);
        await db.SaveChangesAsync();
    }
}
