using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TaskFlow.API.Contracts;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Core.Interfaces;
using TaskFlow.Infrastructure.Persistence;
using TaskEntity = TaskFlow.Core.Domain.Entities.Task;

namespace TaskFlow.API.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks")]
public sealed class TasksController : ControllerBase
{
    private readonly ITaskRepository _taskRepository;
    private readonly AppDbContext _dbContext;

    public TasksController(ITaskRepository taskRepository, AppDbContext dbContext)
    {
        _taskRepository = taskRepository;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TaskResponse>>>> GetAll([FromQuery] Guid? projectId, CancellationToken cancellationToken)
    {
        var tasks = await _taskRepository.GetAllAsync(projectId, cancellationToken);
        var response = tasks.Select(Map).ToList();
        return Ok(ApiResponse<IReadOnlyList<TaskResponse>>.Ok(response, "Tasks retrieved."));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TaskResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return NotFound(ApiResponse<TaskResponse>.Fail("Task not found."));
        }

        return Ok(ApiResponse<TaskResponse>.Ok(Map(task), "Task retrieved."));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TaskResponse>>> Create(TaskCreateRequest request, CancellationToken cancellationToken)
    {
        var projectExists = await _dbContext.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken);
        if (!projectExists)
        {
            return NotFound(ApiResponse<TaskResponse>.Fail("Project not found."));
        }

        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Title = request.Title,
            Content = request.Content,
            Status = request.Status,
            Priority = request.Priority,
            DueDate = request.DueDate
        };

        await _taskRepository.CreateAsync(task, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, ApiResponse<TaskResponse>.Ok(Map(task), "Task created."));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TaskResponse>>> Update(Guid id, TaskUpdateRequest request, CancellationToken cancellationToken)
    {
        var task = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (task is null)
        {
            return NotFound(ApiResponse<TaskResponse>.Fail("Task not found."));
        }

        var projectExists = await _dbContext.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken);
        if (!projectExists)
        {
            return NotFound(ApiResponse<TaskResponse>.Fail("Project not found."));
        }

        task.ProjectId = request.ProjectId;
        task.Title = request.Title;
        task.Content = request.Content;
        task.Status = request.Status;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;

        await _taskRepository.UpdateAsync(task, cancellationToken);

        return Ok(ApiResponse<TaskResponse>.Ok(Map(task), "Task updated."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var task = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (task is null)
        {
            return NotFound(ApiResponse<object?>.Fail("Task not found."));
        }

        await _taskRepository.DeleteAsync(task, cancellationToken);

        return Ok(ApiResponse<object?>.Ok(null, "Task deleted."));
    }

    private static TaskResponse Map(TaskEntity task)
    {
        return new TaskResponse
        {
            Id = task.Id,
            ProjectId = task.ProjectId,
            Title = task.Title,
            Content = task.Content,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate
        };
    }
}
