using Frontend.Data.DTOs;

namespace Frontend.Services;

public interface IProfileService
{
    Task<List<CategoryDto>> GetFormForUserAsync(Guid userId);
    Task<SaveResult> SaveAsync(Guid studentId, List<FieldValueInput> values);
    Task<List<CategoryDto>> GetProfileAsync(Guid userId);
    Task<bool> HasProfileAsync(Guid studentId);
    Task DeleteSteckbriefAsync(Guid userId);
}