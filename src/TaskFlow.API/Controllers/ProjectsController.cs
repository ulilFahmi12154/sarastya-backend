using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TaskFlow.API.Contracts;
using TaskFlow.Application.DTOs.Projects;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Core.Domain.Entities;
using TaskFlow.Core.Interfaces;
using TaskFlow.Infrastructure.Persistence;
using TaskEntity = TaskFlow.Core.Domain.Entities.Task;

namespace TaskFlow.API.Controllers;

[ApiController]
[Authorize]
[Route("api/projects")]
public sealed class ProjectsController : ControllerBase
{
    private readonly IProjectRepository _projectRepository;
    private readonly AppDbContext _dbContext;

    public ProjectsController(IProjectRepository projectRepository, AppDbContext dbContext)
    {
        _projectRepository = projectRepository;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProjectResponse>>>> GetAll(CancellationToken cancellationToken)
    {
        var projects = await _projectRepository.GetAllAsync(cancellationToken);
        var response = projects.Select(Map).ToList();
        return Ok(ApiResponse<IReadOnlyList<ProjectResponse>>.Ok(response, "Projects retrieved."));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProjectResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
        if (project is null)
        {
            return NotFound(ApiResponse<ProjectResponse>.Fail("Project not found."));
        }

        return Ok(ApiResponse<ProjectResponse>.Ok(Map(project), "Project retrieved."));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProjectResponse>>> Create(ProjectCreateRequest request, CancellationToken cancellationToken)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _projectRepository.CreateAsync(project, cancellationToken);
        var response = Map(project);

        return CreatedAtAction(nameof(GetById), new { id = project.Id }, ApiResponse<ProjectResponse>.Ok(response, "Project created."));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProjectResponse>>> Update(Guid id, ProjectUpdateRequest request, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (project is null)
        {
            return NotFound(ApiResponse<ProjectResponse>.Fail("Project not found."));
        }

        project.Name = request.Name;
        project.Description = request.Description;
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;

        await _projectRepository.UpdateAsync(project, cancellationToken);

        return Ok(ApiResponse<ProjectResponse>.Ok(Map(project), "Project updated."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (project is null)
        {
            return NotFound(ApiResponse<object?>.Fail("Project not found."));
        }

        await _projectRepository.DeleteAsync(project, cancellationToken);

        return Ok(ApiResponse<object?>.Ok(null, "Project deleted."));
    }

    private static ProjectResponse Map(Project project)
    {
        return new ProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            CreatedAt = project.CreatedAt,
            Tasks = project.Tasks?.Select(Map).ToList() ?? new List<TaskResponse>()
        };
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
