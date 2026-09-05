# Editing, Completing, and Showing Everything

**What was built**: the task list now shows every field (Recipient, Project, Source+ticket); a checkbox toggles `IsComplete`; an "Edit" button reopens `AddTaskDialog` pre-filled against the same task.

## Binding to the whole object when a converter needs more than one property

`RequesterToSummaryConverter`/`RecipientToSummaryConverter`/`ProjectToSummaryConverter` each bind to one property (`{Binding Requester, Converter=...}`). The Source summary needs two — `Source.Name` and `TaskItem.TicketNumber` together — and WinUI has no `MultiBinding` (unlike WPF). The fix is binding the converter to the whole item instead of a single property:

```xml
<TextBlock Text="{Binding Converter={StaticResource TaskSourceSummaryConverter}}" />
```

A `{Binding}` with no `Path` binds to the `DataContext` itself — here, the `TaskItem` — so the converter's `Convert(object value, ...)` receives the whole object and can read as many of its properties as it needs.

## One dialog, two jobs: create and edit

Rather than a near-duplicate `EditTaskDialog`, `AddTaskDialog` took an optional `TaskItem? existing` constructor parameter. When present, it pre-fills every field (including re-selecting the matching `Person`/`Project`/`Source` by `Id` in each `ComboBox`) and switches `Title`/`PrimaryButtonText` to "Edit Task"/"Save". Critically, `PrimaryButtonClick` mutates `_existing` **in place** instead of building a `new TaskItem()`:

```csharp
var task = _existing ?? new TaskItem();
task.Title = title;
// ...
Result = task;
```

This matters because `Result` needs to carry the task's original `Id` back to the caller for `TaskRepository.UpdateAsync` to find the right row — a fresh `TaskItem` would have `Id = 0` and get inserted as a new row instead of updating the existing one.

## Forcing a list to notice an in-place mutation

`TaskItem` deliberately isn't observable (`CONTEXT.md` — plain data, no framework coupling). But `ObservableCollection<TaskItem>` only tells the `ListView` "something changed" when *its own* contents change (an item added/removed/replaced) — not when a property *inside* an item you already mutated changes. Editing `task.Title` in place and leaving it at the same index would leave the `ListView` showing stale text forever, since nothing tells the binding engine to re-read it.

The fix, in `TasksViewModel.UpdateTaskAsync`:

```csharp
var index = Tasks.IndexOf(task);
if (index >= 0)
{
    Tasks[index] = task;   // same reference — but this still raises CollectionChanged(Replace)
}
```

Setting an `ObservableCollection` element via its indexer always raises `CollectionChanged` with `Replace`, even when the new value is reference-equal to what was already there. `ListView` responds by re-realizing that row's container, which re-evaluates every binding against the item's *current* property values — achieving the same visible effect as if `TaskItem` had raised `PropertyChanged`, without making the model observable. The complete/incomplete checkbox uses the exact same `UpdateTaskAsync` call, since toggling `IsComplete` is just a smaller edit.

## Updating a disconnected entity graph safely

`TaskRepository.UpdateAsync` has the same problem `AddAsync` did (`Documentation/Walkthroughs/06`), from the opposite direction: `db.Tasks.Update(task)` marks the *entire reachable graph* as `Modified` by default — including `task.Requester`, which hasn't actually changed and is definitely not something we want an `UPDATE` statement touching. The fix is the same `AttachIfExisting` helper, called *before* `Update`:

```csharp
AttachIfExisting(db, task.Requester);   // marks it Unchanged, already tracked
// ...
db.Tasks.Update(task);                  // graph walk finds Requester already tracked, leaves it alone
```

`TaskRepositoryTests.UpdateAsync_PersistsChanges_WithoutDuplicatingAnAlreadySavedRequester` exercises this against a real SQLite database — the same reasoning as the `AddAsync` test, just for the update path.
