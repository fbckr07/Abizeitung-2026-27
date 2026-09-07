using Frontend.Data.DTOs;
using Frontend.Data.Entities;

namespace Frontend.Services;

public interface ITeacherManagementService
{
    Task<IReadOnlyList<TeacherListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(string name, Gender gender, CancellationToken cancellationToken = default);

    Task UpdateAsync(Guid id, string name, Gender gender, CancellationToken cancellationToken = default);
    
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
