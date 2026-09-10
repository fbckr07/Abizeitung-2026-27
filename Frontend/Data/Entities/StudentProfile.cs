namespace Frontend.Data.Entities;

public sealed class StudentProfile
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public ICollection<ProfileValue> Values { get; set; } = new List<ProfileValue>();
}