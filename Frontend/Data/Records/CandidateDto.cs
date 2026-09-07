namespace Frontend.Data.Records;

public sealed record CandidateDto(Guid Id, string Name, string? Course = null);