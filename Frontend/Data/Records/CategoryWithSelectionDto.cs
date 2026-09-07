using Frontend.Data.Entities;

namespace Frontend.Data.Records;

public sealed class CategoryWithSelectionDto<TKandidat> where TKandidat : class, ICandidate
{
    public Guid CategoryId { get; init; }

    public Guid QuestionGroupId { get; init; }

    public Gender Gender { get; init; }

    public required string QuestionText { get; init; }
    
    public bool IsDuo { get; init; }

    public bool IsCrossGender { get; init; }
    
    public TKandidat? PreviousSelection { get; init; }
    
    public TKandidat? PreviousSelection2 { get; init; }
}