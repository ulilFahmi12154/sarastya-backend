namespace TaskFlow.Core.Domain.Entities;

public sealed class Task
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public int Priority { get; set; }
    public DateTime? DueDate { get; set; }

    public Project? Project { get; set; }
}

public enum TaskStatus
{
    Todo = 0,
    Doing = 1,
    Done = 2
}
