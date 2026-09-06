using Microsoft.EntityFrameworkCore;
using VantageFlow.Core.Modules.TaskManager.Models;

namespace VantageFlow.Core.Modules.TaskManager.Data;

public sealed class TaskManagerDbContext(DbContextOptions<TaskManagerDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Source> Sources => Set<Source>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>(task =>
        {
            // Stored as text, not the underlying int, so reordering the enum later can't
            // silently change what an existing row means.
            task.Property(t => t.Commitment).HasConversion<string>();

            // Derived properties (see TaskItem) — custom get/set logic or no setter at all, not
            // stored facts, so none of these may become their own columns.
            task.Ignore(t => t.IsStarted);
            task.Ignore(t => t.IsComplete);
            task.Ignore(t => t.State);

            // TaskItem has two references to Person (Requester, Recipient); EF Core can't infer
            // which foreign key belongs to which navigation without this. Shadow FK properties
            // (declared here, not on TaskItem) keep persistence plumbing out of the domain model.
            task.HasOne(t => t.Requester).WithMany().HasForeignKey("RequesterId");
            task.HasOne(t => t.Recipient).WithMany().HasForeignKey("RecipientId");
            task.HasOne(t => t.Project).WithMany().HasForeignKey("ProjectId");
            task.HasOne(t => t.Source).WithMany().HasForeignKey("SourceId");
        });
    }
}
