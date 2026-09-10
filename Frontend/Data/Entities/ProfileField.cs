using Frontend.Data.Enums;

namespace Frontend.Data.Entities;

public class ProfileField
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public ProfileCategory Category { get; set; } = default!;
    
    public string Label { get; set; } = default!;
    public string? Placeholder { get; set; }
    public FieldType Type { get; set; }
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
    
    public List<ProfileFieldOption> Options { get; set; } = new List<ProfileFieldOption>();
}