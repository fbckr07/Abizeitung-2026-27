using Frontend.Data;
using Frontend.Data.DTOs;
using Frontend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frontend.Services;

public sealed class TeacherQuoteLikeService(IDbContextFactory<AppDbContext> dbFactory) : ITeacherQuoteLikeService
{
    public async Task<bool> ToggleLikeAsync(Guid teacherQuoteId, Guid studentId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var existing = await db.TeacherQuoteLikes
            .FirstOrDefaultAsync(l => l.TeacherQuoteId == teacherQuoteId && l.StudentId == studentId, ct);

        if (existing is not null)
        {
            db.TeacherQuoteLikes.Remove(existing);
            await db.SaveChangesAsync(ct);
            return false; // Like removed
        }

        db.TeacherQuoteLikes.Add(new TeacherQuoteLike
        {
            TeacherQuoteId = teacherQuoteId,
            StudentId = studentId,
            CreatedAt = DateTime.UtcNow
        });

        try
        {
            await db.SaveChangesAsync(ct);
            return true; // Like added
        }
        catch (DbUpdateException e)
        {
            // Race Condition: Another like was added concurrently, ignore the exception
            return true;
        }
    }

    public async Task<int> GetLikeCountAsync(Guid teacherQuoteId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.TeacherQuoteLikes.CountAsync(l => l.TeacherQuoteId == teacherQuoteId, ct);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetLikeCountsAsync(IEnumerable<Guid> teacherQuoteIds, CancellationToken ct = default)
    {
        var ids = teacherQuoteIds.ToArray();
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        return await db.TeacherQuoteLikes
            .Where(l => ids.Contains(l.TeacherQuoteId))
            .GroupBy(l => l.TeacherQuoteId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);
    }

    public async Task<IReadOnlySet<Guid>> GetLikedQuoteIdsAsync(Guid studentId, IEnumerable<Guid> teacherQuoteIds, CancellationToken ct = default)
    {
        var ids = teacherQuoteIds.ToArray();
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var liked = await db.TeacherQuoteLikes
            .Where(l => l.StudentId == studentId && ids.Contains(l.TeacherQuoteId))
            .Select(l => l.TeacherQuoteId)
            .ToListAsync(ct);

        return liked.ToHashSet();
    }

    public async Task<IReadOnlyList<QuoteLikeStatDto>> GetQuoteLikeLeaderboardAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        
        return await db.TeacherQuotes
            .Where(l => l.IsReleased)
            .Include(q => q.Teacher)
            .OrderByDescending(q => q.Likes.Count)
            .ThenBy(q => q.Teacher.Name)
            .Select(q => new QuoteLikeStatDto(
                q.Id,
                q.Teacher.Name,
                q.Quote,
                q.Context,
                q.IsReleased,
                q.Likes.Count))
            .ToListAsync(ct);
    }
}