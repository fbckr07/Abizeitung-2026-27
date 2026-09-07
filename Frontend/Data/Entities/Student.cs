namespace Frontend.Data.Entities;

public sealed class Student : ICandidate
{
    public Guid Id { get; set; }
    
    public int OldId { get; set; }
    public int StudentNumber { get; set; }

    public required string Name { get; set; }
    
    public string? Course { get; set; }
    
    public Gender Gender { get; set; }
    
    public required string LoginCode { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<string>? Permissions { get; set; } = new List<string>();
    
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
