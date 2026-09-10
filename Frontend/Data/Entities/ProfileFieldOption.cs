namespace Frontend.Data.Entities;

public class ProfileFieldOption
{
    public Guid Id { get; set; }
    public Guid FieldId { get; set; }
    public ProfileField Field { get; set; } = default!;
    
    public string Value { get; set; } = default!;
    public string Label { get; set; } = default!;
    public int SortOrder { get; set; }
}