using Frontend.Data.DTOs;

namespace Frontend.Services;

public interface IModerationService
{
    Task<IReadOnlyList<OpenQuoteDto>> GetOpenQuotesAsync(CancellationToken cancellationToken = default);

    Task AcceptQuoteAsync(Guid quoteId, CancellationToken cancellationToken = default);
    
    Task DenyQuoteAsync(Guid quoteId, CancellationToken cancellationToken = default);
}
