using Microsoft.EntityFrameworkCore;
using VantageFlow.Core.Modules.TaskManager.Data;
using VantageFlow.Core.Modules.TaskManager.Models;

namespace VantageFlow.Core.Modules.TaskManager.Services;

public sealed class TaskRepository(IDbContextFactory<TaskManagerDbContext> contextFactory) : ITaskRepository
{
    public async Task<IReadOnlyList<TaskItem>> GetAllAsync()
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.Tasks
            .Include(t => t.Requester)
            .Include(t => t.Recipient)
            .Include(t => t.Project)
            .Include(t => t.Source)
            .ToListAsync();
    }

    public async Task AddAsync(TaskItem task)
    {
        await using var db = await contextFactory.CreateDbContextAsync();

        // Each call uses a fresh, short-lived DbContext (the recommended DI pattern for a
        // desktop app with no natural per-request scope), so a Requester/Recipient/Project/
        // Source selected earlier via a different DbContext is "disconnected" — EF Core has no
        // way to know it already exists. Attaching it as Unchanged tells EF to reference the
        // existing row by its Id instead of trying to insert a duplicate.
        AttachIfExisting(db, task.Requester);
        AttachIfExisting(db, task.Recipient);
        AttachIfExisting(db, task.Project);
        AttachIfExisting(db, task.Source);

        db.Tasks.Add(task);
        await db.SaveChangesAsync();
    }

    private static void AttachIfExisting<TEntity>(TaskManagerDbContext db, TEntity? entity)
        where TEntity : class
    {
        if (entity is not null && db.Entry(entity).State == EntityState.Detached)
        {
            db.Attach(entity);
        }
    }
}
