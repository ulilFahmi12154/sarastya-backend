using TaskFlow.Core.Domain.Entities;

namespace TaskFlow.Core.Interfaces;

public interface IProjectRepository
{
    System.Threading.Tasks.Task<IReadOnlyList<Project>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<Project?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<Guid> CreateAsync(Project project, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task UpdateAsync(Project project, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task DeleteAsync(Project project, CancellationToken cancellationToken = default);
}
