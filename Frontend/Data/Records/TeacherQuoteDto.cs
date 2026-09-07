namespace Frontend.Data.Records;

public sealed record TeacherQuoteDto(
    Guid Id,
    Guid TeacherId,
    string TeacherName,
    string Quote,
    string? Context,
    DateTime CreatedAt);

public sealed record TeacherQuotePage(IReadOnlyList<TeacherQuoteDto> Quotes, int TotalCount);