using Frontend.Data.Entities;
using Frontend.Data.Enums;

namespace Frontend.Services;

public interface ICategoryAdminService
{
    Task<List<ProfileCategory>> GetAllCategoriesAsync(bool includeInactive = false);
    Task<ProfileCategory?> GetCategoryByIdAsync(Guid categoryId);
    Task<ProfileCategory> CreateCategoryAsync(string name, string? description);
    Task UpdateCategoryAsync(Guid categoryId, string name, string? description);
    Task SetCategoryActiveAsync(Guid categoryId, bool isActive);
    Task<bool> DeleteCategoryAsync(Guid categoryId); // false, falls Felder mit Werten existieren
    Task ReorderCategoriesAsync(IEnumerable<(Guid Id, int SortOrder)> order);
    
    Task<ProfileField?> GetFieldByIdAsync(Guid fieldId);
    Task<List<ProfileField>> GetFieldsByCategoryAsync(Guid categoryId);
    Task<ProfileField> AddFieldAsync(Guid categoryId, string label, FieldType type, bool isRequired,
        string? placeholder = null);
    Task UpdateFieldAsync(Guid fieldId, string label, bool isRequired, string? placeholder = null);
    Task<bool> ChangeFieldTypeAsync(Guid fieldId, FieldType newType); // false, falls bereits Werte existieren
    Task<bool> DeleteFieldAsync(Guid fieldId); // false, falls bereits Werte existieren
    Task ReorderFieldsAsync(Guid categoryId, IEnumerable<(Guid Id, int SortOrder)> order);
    
    Task<ProfileFieldOption> AddOptionAsync(Guid fieldId, string value, string label);
    Task UpdateOptionAsync(Guid optionId, string value, string label);
    Task<bool> DeleteOptionAsync(Guid optionId); // false, falls Option bereits ausgewählt wurde
    Task ReorderOptionsAsync(Guid fieldId, IEnumerable<(Guid Id, int SortOrder)> order);
}