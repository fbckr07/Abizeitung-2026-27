namespace Frontend.Data.DTOs;

public record QuoteLikeStatDto(
    Guid Id,
    string TeacherName,
    string Quote,
    string? Context,
    bool IsReleased,
    int LikeCount);