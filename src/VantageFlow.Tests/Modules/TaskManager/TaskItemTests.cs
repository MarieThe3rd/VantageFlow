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
}
