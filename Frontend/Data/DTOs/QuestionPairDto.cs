namespace Frontend.Data.DTOs;

public sealed record QuestionPairDto(
    Guid QuestionGroupId,
    Guid IdMale,
    Guid IdFemale,
    string TextMale,
    string TextFemale,
    bool IsActive,
    DateTime? ActiveUntil,
    bool HasVotes,
    bool IsDuo = false,
    bool IsCrossGender = false);