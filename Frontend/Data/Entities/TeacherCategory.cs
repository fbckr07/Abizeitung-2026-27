namespace Frontend.Data.Entities;

public sealed class TeacherCategory
{
    public Guid Id { get; set; }
    
    public Guid QuestionGroupId { get; set; }

    public Gender Gender { get; set; }

    public required string QuestionText { get; set; }

    public bool IsActive { get; set; }

    public DateTime? ActiveUntil { get; set; }
}
