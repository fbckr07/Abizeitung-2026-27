using Frontend.Data;
using Frontend.Data.DTOs;
using Frontend.Data.Entities;
using Frontend.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace Frontend.Services;

public class ProfileService(AppDbContext db, IProfileValidator validator) : IProfileService
{
    public async Task<List<CategoryDto>> GetFormForUserAsync(Guid userId)
    {
        var categories = await db.ProfileCategories
            .Where(c => c.IsActive)
            .Include(c => c.Fields.OrderBy(f => f.SortOrder))
            .ThenInclude(f => f.Options.OrderBy(o => o.SortOrder))
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        var profile = await db.StudentProfiles
            .Include(u => u.Values)
            .ThenInclude(v => v.SelectedOptions)
            .FirstOrDefaultAsync(u => u.StudentId == userId);

        return MapToCategoryDtos(categories, profile);
    }

    public async Task<List<CategoryDto>> GetProfileAsync(Guid userId)
    {
        return await GetFormForUserAsync(userId);
    }

    public async Task<bool> HasProfileAsync(Guid studentId)
    {
        return await db.StudentProfiles.AnyAsync(u => u.StudentId == studentId);
    }
    
    public async Task<SaveResult> SaveAsync(Guid studentId, List<FieldValueInput> values)
    {
        var fields = await db.ProfileFields
            .Where(f => f.Category.IsActive)
            .Include(f => f.Options)
            .ToListAsync();

        var errors = validator.Validate(fields, values);
        if (errors.Count > 0)
        {
            return new SaveResult { Success = false, Errors = errors };
        }

        var studentProfile = await db.StudentProfiles
            .Include(u => u.Values)
                .ThenInclude(v => v.SelectedOptions)
            .FirstOrDefaultAsync(u => u.StudentId == studentId);

        if (studentProfile is null)
        {
            studentProfile = new StudentProfile
            {
                StudentId = studentId,
                CreatedAt = DateTime.UtcNow
            };
            db.StudentProfiles.Add(studentProfile);
        }
        else
        {
            studentProfile.UpdatedAt = DateTime.UtcNow;
        }

        var fieldsById = fields.ToDictionary(f => f.Id);

        foreach (var input in values)
        {
            if (!fieldsById.TryGetValue(input.FieldId, out var field))
                continue; // Feld existiert nicht (mehr) -> ignorieren

            var existingValue = studentProfile.Values.FirstOrDefault(v => v.FieldId == input.FieldId);

            if (existingValue is null)
            {
                existingValue = new ProfileValue()
                {
                    Id = Guid.NewGuid(),
                    StudentProfileId = studentProfile.Id,
                    FieldId = input.FieldId
                };
                studentProfile.Values.Add(existingValue);
                db.ProfileValues.Add(existingValue);
            }

            if (field.Type == FieldType.MultiSelect)
            {
                existingValue.Value = null;
                SyncSelectedOptions(existingValue, input.SelectedOptionIds ?? new List<Guid>());
            }
            else if (field.Type == FieldType.SingleSelect)
            {
                existingValue.Value = input.SelectedOptionIds?.FirstOrDefault().ToString();
                SyncSelectedOptions(existingValue, input.SelectedOptionIds ?? new List<Guid>());
            }
            else
            {
                existingValue.Value = input.Value;
                SyncSelectedOptions(existingValue, new List<Guid>()); // ggf. alte Selects aufräumen
            }
        }

        await db.SaveChangesAsync();
        return new SaveResult { Success = true };
    }
    
    public async Task DeleteSteckbriefAsync(Guid userId)
    {
        var studentProfile = await db.StudentProfiles
            .FirstOrDefaultAsync(u => u.StudentId == userId);

        if (studentProfile is not null)
        {
            db.StudentProfiles.Remove(studentProfile); // Cascade löscht Values + ValueOptions mit
            await db.SaveChangesAsync();
        }
    }
    
    private void SyncSelectedOptions(ProfileValue value, List<Guid> selectedOptionIds)
    {
        var toRemove = value.SelectedOptions
            .Where(vo => !selectedOptionIds.Contains(vo.FieldOptionId))
            .ToList();
        foreach (var remove in toRemove)
        {
            value.SelectedOptions.Remove(remove);
            db.ProfileValueOptions.Remove(remove);
        }

        var existingIds = value.SelectedOptions.Select(vo => vo.FieldOptionId).ToHashSet();
        foreach (var optionId in selectedOptionIds.Where(id => !existingIds.Contains(id)))
        {
            var newSelection = new ProfileValueOption()
            {
                Id = Guid.NewGuid(),
                ProfileValueId = value.Id,
                FieldOptionId = optionId
            };
            value.SelectedOptions.Add(newSelection);
            db.ProfileValueOptions.Add(newSelection);
        }
    }


    private List<CategoryDto> MapToCategoryDtos(List<ProfileCategory> categories, StudentProfile? studentProfile)
    {
        var valuesByFieldId = studentProfile?.Values.ToDictionary(v => v.FieldId)
                              ?? new Dictionary<Guid, ProfileValue>();

        return categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Fields = c.Fields.Select(f =>
            {
                valuesByFieldId.TryGetValue(f.Id, out var existingValue);

                return new FieldDto
                {
                    Id = f.Id,
                    Label = f.Label,
                    Placeholder = f.Placeholder,
                    Type = f.Type,
                    IsRequired = f.IsRequired,
                    Options = f.Options.Select(o => new FieldOptionDto
                    {
                        Id = o.Id,
                        Value = o.Value,
                        Label = o.Label
                    }).ToList(),
                    Value = existingValue?.Value,
                    SelectedOptionIds = existingValue?.SelectedOptions.Select(vo => vo.FieldOptionId).ToList()
                                        ?? new List<Guid>()
                };
            }).ToList()
        }).ToList();
    }
}