using Frontend.Data;
using Frontend.Data.Entities;
using Frontend.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace Frontend.Services;

public class CategoryAdminService(IDbContextFactory<AppDbContext> dbFactory) : ICategoryAdminService
{
    public async Task<List<ProfileCategory>> GetAllCategoriesAsync(bool includeInactive = false)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var query = db.ProfileCategories
            .Include(c => c.Fields.OrderBy(f => f.SortOrder))
            .ThenInclude(f => f.Options.OrderBy(o => o.SortOrder))
            .AsQueryable();

        if (!includeInactive)
            query = query.Where(c => c.IsActive);

        return await query
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
    }
    
    public async Task<ProfileCategory?> GetCategoryByIdAsync(Guid categoryId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.ProfileCategories
            .Include(c => c.Fields.OrderBy(f => f.SortOrder))
            .ThenInclude(f => f.Options.OrderBy(o => o.SortOrder))
            .FirstOrDefaultAsync(c => c.Id == categoryId);
    }

    public async Task<ProfileCategory> CreateCategoryAsync(string name, string? description)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var maxSortOrder = await db.ProfileCategories.AnyAsync()
            ? await db.ProfileCategories.MaxAsync(c => c.SortOrder)
            : 0;

        var category = new ProfileCategory
        {
            Name = name,
            Description = description ?? string.Empty,
            SortOrder = maxSortOrder + 1,
            IsActive = true
        };

        db.ProfileCategories.Add(category);
        await db.SaveChangesAsync();
        return category;
    }
    
    public async Task UpdateCategoryAsync(Guid categoryId, string name, string? description)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var category = await db.ProfileCategories.FindAsync(categoryId)
                       ?? throw new InvalidOperationException($"Kategorie {categoryId} nicht gefunden.");

        category.Name = name;
    category.Description = description ?? string.Empty;
        await db.SaveChangesAsync();
    }

    public async Task SetCategoryActiveAsync(Guid categoryId, bool isActive)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var category = await db.ProfileCategories.FindAsync(categoryId)
                       ?? throw new InvalidOperationException($"Category with ID {categoryId} not found.");

        category.IsActive = isActive;
        await db.SaveChangesAsync();
    }

    public async Task<bool> DeleteCategoryAsync(Guid categoryId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var hasValues = await db.ProfileValues
            .AnyAsync(v => v.Field.CategoryId == categoryId);

        if (hasValues)
            return false;

        var category = await db.ProfileCategories.FindAsync(categoryId);
        if (category is null) return false;
        
        db.ProfileCategories.Remove(category);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task ReorderCategoriesAsync(IEnumerable<(Guid Id, int SortOrder)> order)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var ids = order.Select(o => o.Id).ToList();
        var categories = await db.ProfileCategories
            .Where(c => ids.Contains(c.Id))
            .ToListAsync();

        foreach (var (id, sortOrder) in order)
        {
            var category = categories.FirstOrDefault(c => c.Id == id);
            if (category != null)
            {
                category.SortOrder = sortOrder;
            }
        }

        await db.SaveChangesAsync();
    }

    public async Task<ProfileField?> GetFieldByIdAsync(Guid fieldId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.ProfileFields
            .Include(f => f.Options.OrderBy(o => o.SortOrder))
            .FirstOrDefaultAsync(f => f.Id == fieldId);
    }
    
    public async Task<List<ProfileField>> GetFieldsByCategoryAsync(Guid categoryId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.ProfileFields
            .Where(f => f.CategoryId == categoryId)
            .Include(f => f.Options.OrderBy(o => o.SortOrder))
            .OrderBy(f => f.SortOrder)
            .ToListAsync();
    }
    
    public async Task<ProfileField> AddFieldAsync(Guid categoryId, string label, FieldType type, bool isRequired,
        string? placeholder = null)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var categoryExists = await db.ProfileCategories.AnyAsync(c => c.Id == categoryId);
        if (!categoryExists)
            throw new InvalidOperationException($"Kategorie {categoryId} nicht gefunden.");

        var maxSortOrder = await db.ProfileFields.Where(f => f.CategoryId == categoryId).AnyAsync()
            ? await db.ProfileFields.Where(f => f.CategoryId == categoryId).MaxAsync(f => f.SortOrder)
            : 0;

        var field = new ProfileField()
        {
            CategoryId = categoryId,
            Label = label,
            Type = type,
            IsRequired = isRequired,
            Placeholder = placeholder,
            SortOrder = maxSortOrder + 1
        };

        db.ProfileFields.Add(field);
        await db.SaveChangesAsync();
        return field;
    }
    
    public async Task UpdateFieldAsync(Guid fieldId, string label, bool isRequired,
        string? placeholder)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var field = await db.ProfileFields.FindAsync(fieldId)
                    ?? throw new InvalidOperationException($"Feld {fieldId} nicht gefunden.");

        field.Label = label;
        field.IsRequired = isRequired;
        field.Placeholder = placeholder;

        await db.SaveChangesAsync();
    }
    
    public async Task<bool> ChangeFieldTypeAsync(Guid fieldId, FieldType newType)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var hasValues = await db.ProfileValues.AnyAsync(v => v.FieldId == fieldId);
        if (hasValues)
            return false; // Typwechsel bei bestehenden Werten ist riskant -> verbieten

        var field = await db.ProfileFields.FindAsync(fieldId);
        if (field is null) return false;

        field.Type = newType;
        await db.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> DeleteFieldAsync(Guid fieldId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var hasValues = await db.ProfileValues.AnyAsync(v => v.FieldId == fieldId);
        if (hasValues)
            return false; // nicht löschbar, solange Werte existieren

        var field = await db.ProfileFields.FindAsync(fieldId);
        if (field is null) return false;

        db.ProfileFields.Remove(field); // Cascade löscht Options mit
        await db.SaveChangesAsync();
        return true;
    }
    
    public async Task ReorderFieldsAsync(Guid categoryId, IEnumerable<(Guid Id, int SortOrder)> order)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var ids = order.Select(o => o.Id).ToList();
        var fields = await db.ProfileFields
            .Where(f => f.CategoryId == categoryId && ids.Contains(f.Id))
            .ToListAsync();

        foreach (var (id, sortOrder) in order)
        {
            var field = fields.FirstOrDefault(f => f.Id == id);
            if (field is not null)
                field.SortOrder = sortOrder;
        }

        await db.SaveChangesAsync();
    }
    
    public async Task<ProfileFieldOption> AddOptionAsync(Guid fieldId, string value, string label)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var field = await db.ProfileFields.FindAsync(fieldId)
                    ?? throw new InvalidOperationException($"Feld {fieldId} nicht gefunden.");

        if (field.Type != FieldType.SingleSelect && field.Type != FieldType.MultiSelect)
            throw new InvalidOperationException("Optionen sind nur für SingleSelect/MultiSelect-Felder erlaubt.");

        var maxSortOrder = await db.ProfileFieldOptions.Where(o => o.FieldId == fieldId).AnyAsync()
            ? await db.ProfileFieldOptions.Where(o => o.FieldId == fieldId).MaxAsync(o => o.SortOrder)
            : 0;

        var option = new ProfileFieldOption
        {
            FieldId = fieldId,
            Value = value,
            Label = label,
            SortOrder = maxSortOrder + 1
        };

        db.ProfileFieldOptions.Add(option);
        await db.SaveChangesAsync();
        return option;
    }
    
    public async Task UpdateOptionAsync(Guid optionId, string value, string label)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var option = await db.ProfileFieldOptions.FindAsync(optionId)
                     ?? throw new InvalidOperationException($"Option {optionId} nicht gefunden.");

        option.Value = value;
        option.Label = label;
        await db.SaveChangesAsync();
    }
    
    public async Task<bool> DeleteOptionAsync(Guid optionId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var isUsed = await db.ProfileValueOptions.AnyAsync(vo => vo.FieldOptionId == optionId);
        if (isUsed)
            return false; // Option wurde bereits von mind. einem User gewählt

        var option = await db.ProfileFieldOptions.FindAsync(optionId);
        if (option is null) return false;

        db.ProfileFieldOptions.Remove(option);
        await db.SaveChangesAsync();
        return true;
    }
    
    public async Task ReorderOptionsAsync(Guid fieldId, IEnumerable<(Guid Id, int SortOrder)> order)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var ids = order.Select(o => o.Id).ToList();
        var options = await db.ProfileFieldOptions
            .Where(o => o.FieldId == fieldId && ids.Contains(o.Id))
            .ToListAsync();

        foreach (var (id, sortOrder) in order)
        {
            var option = options.FirstOrDefault(o => o.Id == id);
            if (option is not null)
                option.SortOrder = sortOrder;
        }

        await db.SaveChangesAsync();
    }
}