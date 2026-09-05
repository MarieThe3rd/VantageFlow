using VantageFlow.Core.Modules.TaskManager.Models;
using VantageFlow.Core.Modules.TaskManager.ViewModels;
using VantageFlow.Tests.Modules.TaskManager.Fakes;

namespace VantageFlow.Tests.Modules.TaskManager;

public class TasksViewModelTests
{
    private static TasksViewModel CreateViewModel() => new(
        new FakeTaskRepository(), new FakePersonRepository(), new FakeProjectRepository(), new FakeSourceRepository());

    [Fact]
    public async Task AddTaskAsync_AppendsToTasks()
    {
        var viewModel = CreateViewModel();
        var task = new TaskItem { Title = "Write the report" };

        await viewModel.AddTaskAsync(task);

        Assert.Same(task, Assert.Single(viewModel.Tasks));
    }

    [Fact]
    public async Task AddPersonAsync_MakesPersonAvailableAsATaskRequester()
    {
        // Regression guard for Documentation/01-decisions-log.md #13: Requester must come from
        // the reusable People list, not be typed per task — this is the mechanism that adds to it.
        var viewModel = CreateViewModel();
        var person = new Person { Name = "Sarah", Relationship = "Manager" };

        await viewModel.AddPersonAsync(person);
        await viewModel.AddTaskAsync(new TaskItem { Title = "Pull together the report", Requester = person });

        Assert.Same(person, Assert.Single(viewModel.People));
        Assert.Same(person, viewModel.Tasks.Single().Requester);
    }

    [Fact]
    public async Task AddProjectAsync_MakesProjectAvailableAsATaskProject()
    {
        var viewModel = CreateViewModel();
        var project = new Project { Name = "Q3 Migration" };

        await viewModel.AddProjectAsync(project);
        await viewModel.AddTaskAsync(new TaskItem { Title = "Migrate the last service", Project = project });

        Assert.Same(project, Assert.Single(viewModel.Projects));
        Assert.Same(project, viewModel.Tasks.Single().Project);
    }

    [Fact]
    public async Task AddSourceAsync_MakesSourceAvailableAsATaskSource()
    {
        var viewModel = CreateViewModel();
        var source = new Source { Name = "ADO Work Item", IsTicket = true };

        await viewModel.AddSourceAsync(source);
        await viewModel.AddTaskAsync(new TaskItem { Title = "Fix the reported bug", Source = source, TicketNumber = "12345" });

        Assert.Same(source, Assert.Single(viewModel.Sources));
        Assert.Equal("12345", viewModel.Tasks.Single().TicketNumber);
    }

    [Fact]
    public async Task LoadAsync_PopulatesEverythingFromRepositories()
    {
        var taskRepository = new FakeTaskRepository();
        var personRepository = new FakePersonRepository();
        var projectRepository = new FakeProjectRepository();
        var sourceRepository = new FakeSourceRepository();

        var person = new Person { Name = "Sarah" };
        await personRepository.AddAsync(person);
        await taskRepository.AddAsync(new TaskItem { Title = "Existing task", Requester = person });

        var viewModel = new TasksViewModel(taskRepository, personRepository, projectRepository, sourceRepository);
        await viewModel.LoadAsync();

        Assert.Single(viewModel.Tasks);
        Assert.Single(viewModel.People);
    }
}
