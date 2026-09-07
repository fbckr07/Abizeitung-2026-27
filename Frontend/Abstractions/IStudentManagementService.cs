using Frontend.Data.DTOs;
using Frontend.Data.Entities;

namespace Frontend.Services;

public interface IStudentManagementService
{
    Task<Student?> GetStudentAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentOverviewDto>> GetOverviewAsync(
        Guid ownStudentId,
        CancellationToken cancellationToken = default);
    
    Task<StudentListItemDto> CreateAsync(string name, string course, Data.Entities.Gender gender,
        CancellationToken cancellationToken = default);
    
    Task SetPermissionAsync(Guid studentId, IReadOnlyList<string> permissions, CancellationToken cancellationToken = default);

    Task AddPermissionForAllAsync(string permission, CancellationToken cancellationToken = default);

    Task SetGenderAsync(Guid studentId, Gender gender, CancellationToken cancellationToken = default);

    Task SetCourseAsync(Guid studentId, string course, CancellationToken cancellationToken = default);

    Task<StudentListItemDto> CreateAsync(string name, string course, Data.Entities.Gender gender, string loginCode,
        CancellationToken cancellationToken = default);

    Task<StudentListItemDto> CreateAsync(Student student, CancellationToken cancellationToken = default);
}
