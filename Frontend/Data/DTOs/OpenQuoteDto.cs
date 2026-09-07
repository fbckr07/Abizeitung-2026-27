namespace Frontend.Data.DTOs;

public sealed record OpenQuoteDto(
    Guid Id,
    Guid TeacherId,
    string TeacherName,
    string Quote,
    string? Context,
    DateTime CreatedAt);