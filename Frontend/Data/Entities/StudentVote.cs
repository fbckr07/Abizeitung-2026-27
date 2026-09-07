namespace Frontend.Data.Entities;

public sealed class StudentVote
{
    public Guid Id { get; set; }

    public Guid CategoryStudentId { get; set; }

    public Guid VoterStudentId { get; set; }
    
    public Guid VotedStudentId { get; set; }
    
    public Guid? VotedStudent2Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public StudentCategory StudentCategory { get; set; } = null!;

    public Student VoterStudent { get; set; } = null!;

    public Student VotedStudent { get; set; } = null!;

    public Student? VotedStudent2 { get; set; }
}
