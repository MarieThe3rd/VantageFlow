using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VantageFlow.Core.Modules.Notes.Models;
using VantageFlow.Core.Modules.Notes.Services;

namespace VantageFlow.Core.Modules.Notes.ViewModels;

public sealed partial class NotesViewModel(INoteRepository noteRepository) : ObservableObject
{
    public ObservableCollection<Note> Notes { get; } = [];

    [RelayCommand]
    public async Task LoadAsync()
    {
        Notes.Clear();
        foreach (var note in await noteRepository.GetAllAsync())
        {
            Notes.Add(note);
        }
    }

    public async Task AddNoteAsync(Note note)
    {
        await noteRepository.AddAsync(note);
        Notes.Add(note);
    }
}
