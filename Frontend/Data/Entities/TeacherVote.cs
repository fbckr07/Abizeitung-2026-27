namespace Frontend.Data.Entities;

public sealed class TeacherVote
{
    public Guid Id { get; set; }

    public Guid CategoryTeacherId { get; set; }

    public Guid VoterStudentId { get; set; }
    
    public Guid VotedTeacherId { get; set; }

    public DateTime CreatedAt { get; set; }

    public TeacherCategory TeacherCategory { get; set; } = null!;

    public Student VoterStudent { get; set; } = null!;

    public Teacher VotedTeacher { get; set; } = null!;
}
