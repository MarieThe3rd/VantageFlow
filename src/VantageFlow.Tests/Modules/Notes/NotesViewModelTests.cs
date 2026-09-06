using VantageFlow.Core.Modules.Notes.Models;
using VantageFlow.Core.Modules.Notes.ViewModels;
using VantageFlow.Tests.Modules.Notes.Fakes;

namespace VantageFlow.Tests.Modules.Notes;

public class NotesViewModelTests
{
    [Fact]
    public async Task AddNoteAsync_AppendsToNotes()
    {
        var viewModel = new NotesViewModel(new FakeNoteRepository());
        var note = new Note { Title = "Idea", Body = "Try the thing" };

        await viewModel.AddNoteAsync(note);

        Assert.Same(note, Assert.Single(viewModel.Notes));
    }

    [Fact]
    public async Task LoadAsync_PopulatesNotesFromRepository()
    {
        var repository = new FakeNoteRepository();
        await repository.AddAsync(new Note { Title = "Existing note", Body = "Already saved" });

        var viewModel = new NotesViewModel(repository);
        await viewModel.LoadAsync();

        Assert.Single(viewModel.Notes);
    }
}
