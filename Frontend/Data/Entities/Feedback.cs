namespace Frontend.Data.Entities;

public class Feedback
{
    public Guid Id  { get; set; }
    public string? Text { get; set; }
    public string? SentBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public FeedbackType  Type { get; set; }
    public bool Archived { get; set; }
}

public enum FeedbackType
{
    Bug,
    NewIdea
}