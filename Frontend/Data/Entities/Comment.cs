namespace Frontend.Data.Entities;

public class Comment
{
    public Guid Id  { get; set; }
    public string Text { get; set; }
    
    public Guid AuthorId { get; set; }
    
    public Guid TargetId  { get; set; }
    public Student Target { get; set; }
    
    public DateTime Date { get; set; }
    public bool Confirmed { get; set; }
    
}