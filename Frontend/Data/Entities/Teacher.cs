namespace Frontend.Data.Entities;

public sealed class Teacher : ICandidate
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public Gender Gender { get; set; }
}
