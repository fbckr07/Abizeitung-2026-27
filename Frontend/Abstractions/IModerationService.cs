using Frontend.Data.DTOs;

namespace Frontend.Services;

public interface IModerationService
{
    Task<IReadOnlyList<OpenProfileDto>> GetOpenProfilesAsync(CancellationToken cancellationToken = default);

    Task AcceptProfileAsync(Guid profileId, CancellationToken cancellationToken = default);
    
    Task RejectProfileAsync(Guid profileId, string? comment, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OpenQuoteDto>> GetOpenQuotesAsync(CancellationToken cancellationToken = default);

    Task AcceptQuoteAsync(Guid quoteId, CancellationToken cancellationToken = default);
    
    Task DenyQuoteAsync(Guid quoteId, CancellationToken cancellationToken = default);
}
