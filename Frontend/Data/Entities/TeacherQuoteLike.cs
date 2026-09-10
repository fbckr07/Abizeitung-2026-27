namespace Frontend.Data.Entities;

public sealed class TeacherQuoteLike
{
    public Guid Id  { get; set; }
    
    public Guid TeacherQuoteId { get; set; }
    
    public Guid StudentId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public TeacherQuote TeacherQuote { get; set; } = null!;
    
    public Student Student { get; set; } = null!;
}