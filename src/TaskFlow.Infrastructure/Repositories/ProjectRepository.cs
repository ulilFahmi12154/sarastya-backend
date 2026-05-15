using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TaskFlow.Core.Domain.Entities;
using TaskFlow.Core.Interfaces;
using TaskFlow.Infrastructure.Persistence;
using TaskEntity = TaskFlow.Core.Domain.Entities.Task;

namespace TaskFlow.Infrastructure.Repositories;

public sealed class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _dbContext;
    private readonly DapperContext _dapperContext;

    public ProjectRepository(AppDbContext dbContext, DapperContext dapperContext)
    {
        _dbContext = dbContext;
        _dapperContext = dapperContext;
    }

    public async System.Threading.Tasks.Task<IReadOnlyList<Project>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                p.id AS "Id",
                p.user_id AS "UserId",
                p.name AS "Name",
                p.description AS "Description",
                p.start_date AS "StartDate",
                p.end_date AS "EndDate",
                p.created_at AS "CreatedAt",
                t.project_id AS "ProjectId",
                t.id AS "Id",
                t.title AS "Title",
                t.content AS "Content",
                t.status AS "Status",
                t.priority AS "Priority",
                t.due_date AS "DueDate"
            FROM projects p
            LEFT JOIN tasks t ON t.project_id = p.id
            WHERE p.user_id = @UserId;
            """;

        using var connection = _dapperContext.CreateConnection();
        var lookup = new Dictionary<Guid, Project>();
        var command = new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken);

        await connection.QueryAsync<Project, TaskEntity, Project>(
            command,
            (project, task) =>
            {
                if (!lookup.TryGetValue(project.Id, out var existing))
                {
                    existing = project;
                    existing.Tasks = new List<TaskEntity>();
                    lookup.Add(existing.Id, existing);
                }

                if (task is not null && task.Id != Guid.Empty)
                {
                    existing.Tasks.Add(task);
                }

                return existing;
            },
            splitOn: "ProjectId");

        return lookup.Values.ToList();
    }

    public async System.Threading.Tasks.Task<Project?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        const string projectSql = """
            SELECT
                p.id AS "Id",
                p.user_id AS "UserId",
                p.name AS "Name",
                p.description AS "Description",
                p.start_date AS "StartDate",
                p.end_date AS "EndDate",
                p.created_at AS "CreatedAt"
            FROM projects p
            WHERE p.id = @Id
                AND p.user_id = @UserId;
            """;

        const string tasksSql = """
            SELECT
                t.id AS "Id",
                t.project_id AS "ProjectId",
                t.title AS "Title",
                t.content AS "Content",
                t.status AS "Status",
                t.priority AS "Priority",
                t.due_date AS "DueDate"
            FROM tasks t
            WHERE t.project_id = @Id
            ORDER BY t.due_date DESC NULLS LAST;
            """;

        using var connection = _dapperContext.CreateConnection();

        var commandProject = new CommandDefinition(
            projectSql,
            new { Id = id, UserId = userId },
            cancellationToken: cancellationToken);

        var project = await connection.QueryFirstOrDefaultAsync<Project>(commandProject);

        if (project is null)
        {
            return null;
        }

        var commandTasks = new CommandDefinition(
            tasksSql,
            new { Id = id },
            cancellationToken: cancellationToken);

        var tasks = await connection.QueryAsync<TaskEntity>(commandTasks);

        project.Tasks = tasks.ToList();

        return project;
    }

    public async System.Threading.Tasks.Task<Guid> CreateAsync(Project project, CancellationToken cancellationToken = default)
    {
        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return project.Id;
    }

    public async System.Threading.Tasks.Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        if (_dbContext.Entry(project).State == EntityState.Detached)
        {
            _dbContext.Projects.Attach(project);
        }

        _dbContext.Entry(project).Property(p => p.Name).IsModified = true;
        _dbContext.Entry(project).Property(p => p.Description).IsModified = true;
        _dbContext.Entry(project).Property(p => p.StartDate).IsModified = true;
        _dbContext.Entry(project).Property(p => p.EndDate).IsModified = true;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task DeleteAsync(Project project, CancellationToken cancellationToken = default)
    {
        _dbContext.Projects.Remove(project);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
