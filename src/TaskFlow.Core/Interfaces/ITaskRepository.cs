using TaskEntity = TaskFlow.Core.Domain.Entities.Task;

namespace TaskFlow.Core.Interfaces;

public interface ITaskRepository
{
    System.Threading.Tasks.Task<TaskEntity?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<IReadOnlyList<TaskEntity>> GetAllAsync(Guid userId, Guid? projectId, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<Guid> CreateAsync(TaskEntity task, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task UpdateAsync(TaskEntity task, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task DeleteAsync(TaskEntity task, CancellationToken cancellationToken = default);
}
