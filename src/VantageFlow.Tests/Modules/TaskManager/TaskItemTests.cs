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
}
