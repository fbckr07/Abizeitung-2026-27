using Frontend.Data.Entities;

namespace Frontend.Data.DTOs;

public sealed record StudentOverviewDto(Guid Id, string Name, string Course, Gender Gender, bool IsOwn);