using DomainTaskStatus = TaskFlow.Core.Domain.Entities.TaskStatus;

namespace TaskFlow.Application.DTOs.Tasks;

public sealed class TaskUpdateRequest
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public DomainTaskStatus Status { get; set; }
    public int Priority { get; set; }
    public DateTime? DueDate { get; set; }
}
