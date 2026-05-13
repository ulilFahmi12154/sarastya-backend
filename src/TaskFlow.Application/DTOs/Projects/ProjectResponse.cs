using TaskFlow.Application.DTOs.Tasks;

namespace TaskFlow.Application.DTOs.Projects;

public sealed class ProjectResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public IReadOnlyCollection<TaskResponse> Tasks { get; set; } = Array.Empty<TaskResponse>();
}
