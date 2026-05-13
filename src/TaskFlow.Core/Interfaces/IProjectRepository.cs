using TaskFlow.Core.Domain.Entities;

namespace TaskFlow.Core.Interfaces;

public interface IProjectRepository
{
    System.Threading.Tasks.Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<Guid> CreateAsync(Project project, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task UpdateAsync(Project project, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task DeleteAsync(Project project, CancellationToken cancellationToken = default);
}
