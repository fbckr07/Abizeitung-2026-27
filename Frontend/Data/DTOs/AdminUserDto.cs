namespace Frontend.Data.DTOs;

public sealed record AdminUserDto(Guid Id, string Username, List<string> Permissions);