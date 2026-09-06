# The Checkbox Crash, and Making TaskItem Observable

**What happened**: marking a task complete threw at runtime — the first bug in this project that only showed up interactively, not in any test. Worth walking through as a real diagnosis, not just the fix.

## The error

```
System.Runtime.InteropServices.COMException
Child collection must not be modified during measure or arrange.
   at ABI.System.Collections.Specialized.NotifyCollectionChangedEventHandler.NativeDelegateWrapper.Invoke(...)
   at System.Collections.ObjectModel.ObservableCollection`1.OnCollectionChanged(...)
   at TasksViewModel.UpdateTaskAsync(...)
   at TasksPage.TaskComplete_Changed(...)
```

The stack trace names the exact mechanism: `UpdateTaskAsync` raising a `CollectionChanged` event (from `Tasks[index] = task` — the "replace to force a rebind" trick from `Documentation/Walkthroughs/08`), called from `TaskComplete_Changed` — a `CheckBox`'s `Checked`/`Unchecked` handler.

## Root cause, not just the trigger

The `CheckBox` that raised this event lives *inside* the `ListView`'s own `DataTemplate` — it's part of the visual tree the `ListView` is currently laying out as a direct consequence of the user toggling it. Replacing that same list's backing collection while the panel is still mid-measure/arrange from that exact toggle is a genuine WinUI reentrancy trap: the panel doesn't expect its data source to change out from under it before it's finished responding to the interaction that triggered the change.

The `Documentation/Walkthroughs/08` "Edit" button never hit this, purely by accident of timing: clicking it opens a modal `ContentDialog` and `await`s it, so by the time `UpdateTaskAsync` runs, the `ListView`'s layout pass from the original button click is long finished. The checkbox has no such pause — `Checked`/`Unchecked` fires, `UpdateTaskAsync` resumes almost immediately after a fast local SQLite write, and the collection gets mutated while the same layout cycle is potentially still settling.

## The fix: stop needing the workaround, not defer it

The tempting quick fix — wrap the mutation in `DispatcherQueue.TryEnqueue(...)` to push it to the next UI tick — would have worked, but only patches the symptom: it's still true that `TaskItem` can't tell anyone it changed, and the next control that mutates it in a slightly different timing context could hit the same wall again.

The actual fix revisits the assumption from `Documentation/Walkthroughs/08`: `TaskItem` doesn't need to be plain data. It's genuinely edited in place while already on screen — that's a real requirement, and `CommunityToolkit.Mvvm` (already a dependency) makes observability nearly free:

```csharp
public sealed partial class TaskItem : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty]
    private bool _isComplete;
    // ...same for Title, Notes, Commitment, Requester, Recipient, Project, Source, TicketNumber, TicketLink
}
```

`[ObservableProperty]` generates a public `IsComplete` property around the `_isComplete` field that calls `SetProperty` — which raises `PropertyChanged` only when the value actually changes. `TasksViewModel.UpdateTaskAsync` no longer touches `Tasks` at all:

```csharp
public async Task UpdateTaskAsync(TaskItem task)
{
    await taskRepository.UpdateAsync(task);
    // No collection mutation needed — the bound properties already notified the UI directly.
}
```

No collection-level event, no reentrancy risk, for the checkbox *or* the edit dialog — one fix covers both call sites, because the actual problem (a mutated-in-place item couldn't tell anyone) was the same in both, even though only one of them had crashed yet.

## Why this didn't need `required` removed again, and didn't need a new migration

`[ObservableProperty]`-generated properties are still plain public properties named exactly what they were before (`Title`, `IsComplete`, ...) — EF Core maps them identically, and the existing migration's column names didn't change, so no new migration was needed. `TaskItem` still has a public parameterless constructor (implicit, since `ObservableObject` doesn't require one), so the `XamlTypeInfo.g.cs` gotcha from `Documentation/Walkthroughs/05` didn't resurface either. All 13 existing tests passed unchanged after the conversion — good evidence the fix only changed *how* property-changed notification works, not any observable behavior the tests already covered.

## What guards against this regressing

`TaskItemTests.MutatingIsComplete_RaisesPropertyChanged` asserts `PropertyChanged` actually fires — a direct regression test for the root cause, not just "the checkbox doesn't crash," which no automated test could check anyway without the FlaUI UI-test tier this project has planned but not yet built.
