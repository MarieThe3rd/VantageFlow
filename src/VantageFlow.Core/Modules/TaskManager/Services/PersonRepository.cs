using Microsoft.EntityFrameworkCore;
using VantageFlow.Core.Modules.TaskManager.Data;
using VantageFlow.Core.Modules.TaskManager.Models;

namespace VantageFlow.Core.Modules.TaskManager.Services;

public sealed class PersonRepository(IDbContextFactory<TaskManagerDbContext> contextFactory) : IPersonRepository
{
    public async Task<IReadOnlyList<Person>> GetAllAsync()
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.People.ToListAsync();
    }

    public async Task AddAsync(Person person)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        db.People.Add(person);
        await db.SaveChangesAsync(); // populates person.Id
    }
}
