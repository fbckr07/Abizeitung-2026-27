namespace Frontend.Data.Entities;

public sealed class TeacherQuote
{
    public Guid Id { get; set; }
    
    public Guid? SubmittedByStudentId { get; set; }

    public Guid TeacherId { get; set; }

    public required string Quote { get; set; }

    public string? Context { get; set; }

    public bool IsReleased { get; set; }

    public DateTime CreatedAt { get; set; }

    public Student? SubmittedByStudent { get; set; }

    public Teacher Teacher { get; set; } = null!;
    
    public ICollection<TeacherQuoteLike> Likes { get; set; } = new List<TeacherQuoteLike>();
}
