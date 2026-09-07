using Frontend.Data;
using Frontend.Data.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Frontend.Services;

public sealed class ModerationService(AppDbContext db) : IModerationService
{
    public async Task<IReadOnlyList<OpenProfileDto>> GetOpenProfilesAsync(
        CancellationToken cancellationToken = default)
    {
        return await db.StudentProfiles
            .AsNoTracking()
            .Where(p => !p.IstFreigegeben)
            .OrderBy(p => p.ErstelltAm)
            .Select(p => new OpenProfileDto(
                p.Id,
                p.StudentId,
                p.Student.Name,
                p.FotoUrl,
                p.Motto,
                p.Hobbys,
                p.Zukunftswunsch,
                p.AdminKommentar,
                p.ErstelltAm))
            .ToListAsync(cancellationToken);
    }

    public async Task AcceptProfileAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var profil = await db.StudentProfiles.FirstOrDefaultAsync(p => p.Id == profileId, cancellationToken)
            ?? throw new InvalidOperationException("Steckbrief nicht gefunden.");

        profil.IstFreigegeben = true;
        profil.AdminKommentar = null;
        profil.AktualisiertAm = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RejectProfileAsync(
        Guid profileId,
        string? comment,
        CancellationToken cancellationToken = default)
    {
        var profil = await db.StudentProfiles.FirstOrDefaultAsync(p => p.Id == profileId, cancellationToken)
            ?? throw new InvalidOperationException("Steckbrief nicht gefunden.");

        profil.IstFreigegeben = false;
        profil.AdminKommentar = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        profil.AktualisiertAm = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }

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
