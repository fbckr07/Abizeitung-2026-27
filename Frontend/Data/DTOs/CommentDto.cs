namespace Frontend.Data.DTOs;

public sealed record CommentDto(
    Guid Id,
    string Text,
    Guid ErstellerId,
    string ErstellerName,
    Guid ZielId,
    DateTime Datum,
    bool Bestätigt);