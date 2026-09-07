using Frontend.Data.Entities;

namespace Frontend.Data.DTOs;

public sealed record StudentListItemDto(Guid Id, string Name, string Course, string LoginCode, Gender Gender,
    IReadOnlyList<string> Permissions);