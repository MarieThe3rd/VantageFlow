using VantageFlow.Core.Modules.TaskManager.Models;

namespace VantageFlow.Tests.Modules.TaskManager;

public class TaskItemTests
{
    [Fact]
    public void NewTask_DefaultsToObligation()
    {
        // Regression guard for the domain decision in Documentation/01-decisions-log.md #14:
        // a task is a real commitment unless explicitly marked otherwise, not the reverse.
        var task = new TaskItem { Title = "Anything" };

        Assert.Equal(Commitment.Obligation, task.Commitment);
    }

    [Fact]
    public void MutatingIsComplete_RaisesPropertyChanged()
    {
        // Regression guard for the crash in Documentation/Walkthroughs/10: TaskItem must stay
        // observable so a bound ListView refreshes from an in-place edit without the caller
        // needing to replace it in its ObservableCollection (which crashed when triggered from
        // a control that's itself part of that list's item template — "Child collection must
        // not be modified during measure or arrange").
        var task = new TaskItem { Title = "Anything" };
        var raisedFor = new List<string?>();
        task.PropertyChanged += (_, e) => raisedFor.Add(e.PropertyName);

        task.IsComplete = true;

        Assert.Contains(nameof(TaskItem.IsComplete), raisedFor);
    }

    [Fact]
    public void SettingIsCompleteTrue_StampsCompletedDateAsToday()
    {
        // CompletedDate is the single source of truth for completion (Documentation/01-decisions-
        // log.md #20) — IsComplete is derived from it, not tracked separately, specifically so
        // the two can never drift out of sync.
        var task = new TaskItem { Title = "Anything" };

        task.IsComplete = true;

        Assert.Equal(DateOnly.FromDateTime(DateTime.Today), task.CompletedDate);
    }

    [Fact]
    public void SettingIsCompleteFalse_ClearsCompletedDate()
    {
        var task = new TaskItem { Title = "Anything", CompletedDate = new DateOnly(2026, 1, 1) };

        task.IsComplete = false;

        Assert.Null(task.CompletedDate);
    }

    [Fact]
    public void SettingCompletedDateDirectly_MakesIsCompleteTrue()
    {
        // Confirms the derivation works both ways — setting a completion date (e.g. to correct
        // it via the edit dialog) is itself what "complete" means, not something tracked separately.
        var task = new TaskItem { Title = "Anything", CompletedDate = new DateOnly(2026, 1, 1) };

        Assert.True(task.IsComplete);
    }

    [Theory]
    [InlineData(false, false, TaskState.NotStarted)]
    [InlineData(true, false, TaskState.InProgress)]
    [InlineData(false, true, TaskState.Completed)]
    [InlineData(true, true, TaskState.Completed)]
    public void State_IsDerivedFromStartedAndCompleted(bool started, bool completed, TaskState expected)
    {
        // State is always derived from StartDate/CompletedDate, never its own stored fact — same
        // reasoning as IsComplete/IsStarted (Documentation/01-decisions-log.md #21) — so it can
        // never disagree with them, including the case where both are set.
        var task = new TaskItem
        {
            Title = "Anything",
            IsStarted = started,
            IsComplete = completed,
        };

        Assert.Equal(expected, task.State);
    }

    [Fact]
    public void MutatingStartDate_RaisesPropertyChangedForStateToo()
    {
        var task = new TaskItem { Title = "Anything" };
        var raisedFor = new List<string?>();
        task.PropertyChanged += (_, e) => raisedFor.Add(e.PropertyName);

        task.IsStarted = true;

        Assert.Contains(nameof(TaskItem.State), raisedFor);
    }
}
