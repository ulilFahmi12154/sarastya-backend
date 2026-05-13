using DomainTaskStatus = TaskFlow.Core.Domain.Entities.TaskStatus;

namespace TaskFlow.Application.DTOs.Tasks;

public sealed class TaskCreateRequest
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public DomainTaskStatus Status { get; set; } = DomainTaskStatus.Todo;
    public int Priority { get; set; }
    public DateTime? DueDate { get; set; }
}
