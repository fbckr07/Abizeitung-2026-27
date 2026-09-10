using Frontend.Data;
using Frontend.Data.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Frontend.Services;

public sealed class ModerationService(AppDbContext db) : IModerationService
{
    public async Task<IReadOnlyList<OpenQuoteDto>> GetOpenQuotesAsync(CancellationToken cancellationToken = default)
    {
        return await db.TeacherQuotes
            .AsNoTracking()
            .Where(z => !z.IsReleased)
            .OrderBy(z => z.CreatedAt)
            .Select(z => new OpenQuoteDto(z.Id, z.TeacherId, z.Teacher.Name, z.Quote, z.Context, z.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task AcceptQuoteAsync(Guid quoteId, CancellationToken cancellationToken = default)
    {
        var quote = await db.TeacherQuotes.FirstOrDefaultAsync(z => z.Id == quoteId, cancellationToken)
            ?? throw new InvalidOperationException("Zitat nicht gefunden.");

        quote.IsReleased = true;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DenyQuoteAsync(Guid quoteId, CancellationToken cancellationToken = default)
    {
        var zitat = await db.TeacherQuotes.FirstOrDefaultAsync(z => z.Id == quoteId, cancellationToken)
            ?? throw new InvalidOperationException("Zitat nicht gefunden.");

        db.TeacherQuotes.Remove(zitat);
        await db.SaveChangesAsync(cancellationToken);
    }
}
