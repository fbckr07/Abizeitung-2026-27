namespace Frontend.Data.DTOs;

public sealed record OpenProfileDto(
    Guid Id,
    Guid StudentId,
    string StudentName,
    string? PhotoUrl,
    string? Motto,
    string? Hobbys,
    string? Zukunftswunsch,
    string? AdminComment,
    DateTime CreatedAt);