using VantageFlow.Core.Modules.TaskManager.Models;

namespace VantageFlow.Core.Modules.TaskManager.Services;

public interface ITaskRepository
{
    Task<IReadOnlyList<TaskItem>> GetAllAsync();

    Task AddAsync(TaskItem task);

    Task UpdateAsync(TaskItem task);
}
