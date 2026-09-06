# Derived Task State, and Filtering Without Reintroducing the Crash

**What was built**: `TaskState` (derived from `StartDate`/`CompletedDate`), an `IsStarted` property mirroring `IsComplete`, and a `TaskFilter`-driven `FilteredTasks` collection the task list actually binds to.

## The same derivation trick, applied a second time

`Documentation/Walkthroughs/11` replaced a stored `IsComplete` bool with a derived one to avoid a two-sources-of-truth bug. The same reasoning extends cleanly to a third state:

```csharp
public TaskState State =>
    CompletedDate.HasValue ? TaskState.Completed :
    StartDate.HasValue ? TaskState.InProgress :
    TaskState.NotStarted;
```

No new stored field, so `State` can never disagree with `StartDate`/`CompletedDate` — there's nothing to disagree, it's computed from them every time it's read. `[NotifyPropertyChangedFor(nameof(State))]` on *both* the `_startDate` and `_completedDate` fields (in addition to each one's own derived bool) makes sure anything bound to `State` re-evaluates no matter which underlying date changed.

## Two collections, on purpose

`TasksViewModel` now has `Tasks` (every loaded task) and `FilteredTasks` (what `TasksPage`'s `ListView` actually binds to). Filtering `Tasks` in place — instead of keeping a second collection — would mean every existing test asserting `Assert.Single(viewModel.Tasks)` after adding a task could start failing depending on whatever filter happened to be selected, for reasons that have nothing to do with what those tests are actually checking. Keeping them separate means "everything that's loaded" and "what's currently visible" are independently reasoned about, in code and in tests.

## A second reentrancy risk — and why the fix is different this time

Toggling "Started" or "Done" can now cross a filter boundary: mark something Completed while viewing "Active," and it needs to *disappear* from `FilteredTasks`. That's a real collection-level change (an item removed), not a property update on a still-visible row — so making `TaskItem` observable (the `Documentation/Walkthroughs/10` fix) doesn't help here; that fix specifically works by *avoiding* a collection mutation, and this feature genuinely needs one.

So the same crash risk from `Documentation/Walkthroughs/10` — the triggering `CheckBox` lives inside the very `ListView` whose backing collection is about to change — is back, but for a legitimate reason this time. The fix is the "quick fix" that was *rejected* in that walkthrough for being a band-aid:

```csharp
private void DeferUpdate(object sender)
{
    if (((FrameworkElement)sender).DataContext is not TaskItem task)
    {
        return;
    }

    DispatcherQueue.TryEnqueue(async () => await ViewModel.UpdateTaskAsync(task));
}
```

`DispatcherQueue.TryEnqueue` posts the callback to run on the next UI dispatch cycle — after the current measure/arrange pass (the one the checkbox toggle itself triggered) has fully finished. Deferring was the wrong fix in `Documentation/Walkthroughs/10` because the mutation there was avoidable entirely; it's the *right* fix here because the mutation is the whole point of the feature. Same symptom, same trigger shape, opposite correct response — worth having both in mind rather than reaching for "just defer it" as a reflex the next time a `ListView`-triggered mutation causes trouble.
