# Task Dates and a Derived Property

**What was built**: `StartDate`, `DueDate` (both `DateOnly?`), and `CompletedDate` — which replaced the previously-stored `IsComplete` bool rather than living alongside it.

## A derived property that still notifies

The interesting part isn't the two new plain date fields — it's turning `IsComplete` from a stored fact into a computed one without breaking the `CheckBox` binding that already depended on it:

```csharp
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(IsComplete))]
private DateOnly? _completedDate;

public bool IsComplete
{
    get => CompletedDate.HasValue;
    set => CompletedDate = value ? DateOnly.FromDateTime(DateTime.Today) : null;
}
```

`IsComplete` is a hand-written property, not a `[ObservableProperty]`-generated one — it has real logic (checking/stamping `CompletedDate`), which the source generator can't express from a single backing field. Left alone, this would silently break the `CheckBox`'s `{Binding IsComplete, Mode=TwoWay}`: when `CompletedDate` changes, nothing would tell WinUI that `IsComplete` (a *different* property, as far as the binding system knows) might have changed too. `[NotifyPropertyChangedFor(nameof(IsComplete))]` on the `CompletedDate` field tells the `[ObservableProperty]` source generator to raise `PropertyChanged("IsComplete")` in `CompletedDate`'s generated setter as well as its own — one attribute, and the binding keeps working exactly as it did when `IsComplete` was a plain stored field.

## Telling EF Core to ignore a property it would otherwise map

`IsComplete` still looks, by convention, like any other mappable property — a public getter and setter — so EF Core would try to give it its own column unless told not to:

```csharp
task.Ignore(t => t.IsComplete);
```

Without this, the database would end up with a redundant, stale `IsComplete` column alongside the real `CompletedDate` one — exactly the two-sources-of-truth problem this change was meant to eliminate, just moved from the C# model into the schema instead.

## Why this needed a real migration, not just a code change

Replacing a stored fact with a derived one is a schema change: the old `IsComplete` column had to go, and three new nullable columns had to appear. `dotnet ef migrations add AddTaskDates` scaffolded exactly that — `DropColumn("IsComplete")`, three `AddColumn<DateOnly>` calls — and warned "an operation was scaffolded that may result in the loss of data," which is accurate and expected: any previously-recorded `IsComplete = true` with no corresponding date is genuinely unrecoverable information, since the column holding it no longer exists after this migration runs. Worth pausing on that warning rather than dismissing it by habit — it's not always this obviously fine.
