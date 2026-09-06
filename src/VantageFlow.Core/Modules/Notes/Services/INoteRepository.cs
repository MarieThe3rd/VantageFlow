using VantageFlow.Core.Modules.Notes.Models;

namespace VantageFlow.Core.Modules.Notes.Services;

public interface INoteRepository
{
    Task<IReadOnlyList<Note>> GetAllAsync();

    Task AddAsync(Note note);
}
