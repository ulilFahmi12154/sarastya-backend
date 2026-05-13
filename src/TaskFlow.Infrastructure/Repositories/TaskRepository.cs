using Dapper;
using System.Linq;
using TaskFlow.Core.Interfaces;
using TaskFlow.Infrastructure.Persistence;
using TaskEntity = TaskFlow.Core.Domain.Entities.Task;

namespace TaskFlow.Infrastructure.Repositories;

public sealed class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _dbContext;
    private readonly DapperContext _dapperContext;

    public TaskRepository(AppDbContext dbContext, DapperContext dapperContext)
    {
        _dbContext = dbContext;
        _dapperContext = dapperContext;
    }

    public async System.Threading.Tasks.Task<IReadOnlyList<TaskEntity>> GetAllAsync(Guid? projectId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                t.id AS "Id",
                t.project_id AS "ProjectId",
                t.title AS "Title",
                t.content AS "Content",
                t.status AS "Status",
                t.priority AS "Priority",
                t.due_date AS "DueDate"
            FROM tasks t
            WHERE (@ProjectId IS NULL OR t.project_id = @ProjectId);
            """;

        using var connection = _dapperContext.CreateConnection();
        var command = new CommandDefinition(sql, new { ProjectId = projectId }, cancellationToken: cancellationToken);
        var tasks = await connection.QueryAsync<TaskEntity>(command);
        return tasks.ToList();
    }

    public async System.Threading.Tasks.Task<TaskEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                t.id AS "Id",
                t.project_id AS "ProjectId",
                t.title AS "Title",
                t.content AS "Content",
                t.status AS "Status",
                t.priority AS "Priority",
                t.due_date AS "DueDate"
            FROM tasks t
            WHERE t.id = @Id;
            """;

        using var connection = _dapperContext.CreateConnection();
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<TaskEntity>(command);
    }

    public async System.Threading.Tasks.Task<Guid> CreateAsync(TaskEntity task, CancellationToken cancellationToken = default)
    {
        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return task.Id;
    }

    public async System.Threading.Tasks.Task UpdateAsync(TaskEntity task, CancellationToken cancellationToken = default)
    {
        _dbContext.Tasks.Update(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task DeleteAsync(TaskEntity task, CancellationToken cancellationToken = default)
    {
        _dbContext.Tasks.Remove(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
