using Dapper;
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

    public async System.Threading.Tasks.Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                p.id AS "Id",
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
            LEFT JOIN tasks t ON t.project_id = p.id;
            """;

        using var connection = _dapperContext.CreateConnection();
        var lookup = new Dictionary<Guid, Project>();
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

        await connection.QueryAsync<Project, TaskEntity, Project>(
            command,
            (project, task) =>
            {
                // Dapper returns one row per task; aggregate tasks into a single Project instance.
                if (!lookup.TryGetValue(project.Id, out var existing))
                {
                    existing = project;
                    existing.Tasks = new List<TaskEntity>();
                    lookup.Add(existing.Id, existing);
                }

                if (task.Id != Guid.Empty)
                {
                    existing.Tasks.Add(task);
                }

                return existing;
            },
            splitOn: "ProjectId");

        return lookup.Values.ToList();
    }

    public async System.Threading.Tasks.Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                p.id AS "Id",
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
            WHERE p.id = @Id;
            """;

        using var connection = _dapperContext.CreateConnection();
        var lookup = new Dictionary<Guid, Project>();
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);

        await connection.QueryAsync<Project, TaskEntity, Project>(
            command,
            (project, task) =>
            {
                // Dapper returns one row per task; aggregate tasks into a single Project instance.
                if (!lookup.TryGetValue(project.Id, out var existing))
                {
                    existing = project;
                    existing.Tasks = new List<TaskEntity>();
                    lookup.Add(existing.Id, existing);
                }

                if (task.Id != Guid.Empty)
                {
                    existing.Tasks.Add(task);
                }

                return existing;
            },
            splitOn: "ProjectId");

        return lookup.Values.FirstOrDefault();
    }

    public async System.Threading.Tasks.Task<Guid> CreateAsync(Project project, CancellationToken cancellationToken = default)
    {
        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return project.Id;
    }

    public async System.Threading.Tasks.Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        _dbContext.Projects.Update(project);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task DeleteAsync(Project project, CancellationToken cancellationToken = default)
    {
        _dbContext.Projects.Remove(project);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
