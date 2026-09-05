using VantageFlow.Core.Modules.TaskManager.Models;

namespace VantageFlow.Core.Modules.TaskManager.Services;

public interface IPersonRepository
{
    Task<IReadOnlyList<Person>> GetAllAsync();

    Task AddAsync(Person person);
}
