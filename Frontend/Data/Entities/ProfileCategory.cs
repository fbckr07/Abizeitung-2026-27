namespace Frontend.Data.Entities;

public class ProfileCategory
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;
    
    public string? Description { get; set; }
    
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    
    public ICollection<ProfileField> Fields { get; set; } = new List<ProfileField>();
}