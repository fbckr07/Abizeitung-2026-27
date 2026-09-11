using Frontend.Data;
using Frontend.Data.Entities;
using Frontend.Data.Enums;
using Frontend.Data.Records;
using Microsoft.EntityFrameworkCore;

namespace Frontend.Services;

public sealed class TeacherQuoteService(IDbContextFactory<AppDbContext> dbFactory) : ITeacherQuoteService
{
    private const int MaxZitatLaenge = 500;

    public async Task<IReadOnlyList<CandidateDto>> GetTeacherAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.Teacher
            .AsNoTracking()
            .OrderBy(l => l.Name)
            .Select(l => new CandidateDto(l.Id, l.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task SubmitAsync(
        Guid teacherId,
        string quote,
        string? context,
        Guid? submittedByStudent,
        CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(quote))
        {
            throw new InvalidOperationException("Das Zitat darf nicht leer sein.");
        }

        if (quote.Length > MaxZitatLaenge)
        {
            throw new InvalidOperationException($"Das Zitat darf maximal {MaxZitatLaenge} Zeichen lang sein.");
        }

        var teacherExisting = await db.Teacher.AnyAsync(l => l.Id == teacherId, cancellationToken);
        if (!teacherExisting)
        {
            throw new InvalidOperationException("Lehrer nicht gefunden.");
        }

        var entry = new TeacherQuote
        {
            Id = Guid.NewGuid(),
            TeacherId = teacherId,
            Quote = quote.Trim(),
            Context = string.IsNullOrWhiteSpace(context) ? null : context.Trim(),
            SubmittedByStudentId = submittedByStudent,
            IsReleased = false,
            CreatedAt = DateTime.UtcNow
        };

        db.TeacherQuotes.Add(entry);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<TeacherQuotePage> GetAcceptedQuotesAsync(
        int page,
        int pageSize,
        TeacherQuoteSortMode sortMode,
        CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

        if (page < 0)
        {
            page = 0;
        }

        if (pageSize <= 0)
        {
            pageSize = 20;
        }

        var query = db.TeacherQuotes
            .AsNoTracking()
            .Where(z => z.IsReleased);

        var totalCount = await query.CountAsync(cancellationToken);

        query = sortMode switch
        {
            TeacherQuoteSortMode.CreatedAtDesc => query.OrderByDescending(z => z.CreatedAt),
            _ => query.OrderBy(z => z.Teacher.Name).ThenByDescending(z => z.CreatedAt)
        };

        var zitate = await query
            .Skip(page * pageSize)
            .Take(pageSize)
            .Select(z => new TeacherQuoteDto(
                z.Id,
                z.TeacherId,
                z.Teacher.Name,
                z.Quote,
                z.Context,
                z.CreatedAt))
            .ToListAsync(cancellationToken);

        return new TeacherQuotePage(zitate, totalCount);
    }
}
