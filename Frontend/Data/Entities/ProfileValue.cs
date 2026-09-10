namespace Frontend.Data.Entities;

public class ProfileValue
{
    public Guid Id { get; set; }
    public Guid StudentProfileId { get; set; }
    public StudentProfile StudentProfile { get; set; } = default!;
    
    public Guid FieldId { get; set; }
    public ProfileField Field { get; set; } = default!;
    
    public string? Value { get; set; }
    
    public ICollection<ProfileValueOption> SelectedOptions { get; set; } = new List<ProfileValueOption>();
}