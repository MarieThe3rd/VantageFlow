using VantageFlow.Core.Modules.TaskManager.Models;

namespace VantageFlow.Core.Modules.TaskManager.Services;

public interface IProjectRepository
{
    Task<IReadOnlyList<Project>> GetAllAsync();

    Task AddAsync(Project project);
}
