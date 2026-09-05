using VantageFlow.Core.Modules.TaskManager.Models;

namespace VantageFlow.Core.Modules.TaskManager.Services;

public interface ISourceRepository
{
    Task<IReadOnlyList<Source>> GetAllAsync();

    Task AddAsync(Source source);
}
