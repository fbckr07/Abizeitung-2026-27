using Frontend.Data.Entities;

namespace Frontend.Data.DTOs;

public sealed record TeacherListItemDto(Guid Id, string Name, Gender Gender, bool KannGeloeschtWerden);