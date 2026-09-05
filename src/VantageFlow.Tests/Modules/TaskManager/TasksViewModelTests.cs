using VantageFlow.Core.Modules.TaskManager.Models;
using VantageFlow.Core.Modules.TaskManager.ViewModels;

namespace VantageFlow.Tests.Modules.TaskManager;

public class TasksViewModelTests
{
    [Fact]
    public void AddTask_AppendsToTasks()
    {
        var viewModel = new TasksViewModel();
        var task = new TaskItem { Title = "Write the report" };

        viewModel.AddTask(task);

        Assert.Same(task, Assert.Single(viewModel.Tasks));
    }

    [Fact]
    public void AddPerson_MakesPersonAvailableAsATaskRequester()
    {
        // Regression guard for Documentation/01-decisions-log.md #13: Requester must come from
        // the reusable People list, not be typed per task — this is the mechanism that adds to it.
        var viewModel = new TasksViewModel();
        var person = new Person { Name = "Sarah", Relationship = "Manager" };

        viewModel.AddPerson(person);
        viewModel.AddTask(new TaskItem { Title = "Pull together the report", Requester = person });

        Assert.Same(person, Assert.Single(viewModel.People));
        Assert.Same(person, viewModel.Tasks.Single().Requester);
    }
}
