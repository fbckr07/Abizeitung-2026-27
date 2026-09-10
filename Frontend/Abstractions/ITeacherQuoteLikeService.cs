using Frontend.Data.DTOs;

namespace Frontend.Services;

public interface ITeacherQuoteLikeService
{
    Task<bool> ToggleLikeAsync(Guid teacherQuoteId, Guid studentId, CancellationToken ct = default);

    Task<int> GetLikeCountAsync(Guid teacherQuoteId, CancellationToken ct = default);

    Task<IReadOnlyDictionary<Guid, int>> GetLikeCountsAsync(
        IEnumerable<Guid> teacherQuoteIds, CancellationToken ct = default);

    Task<IReadOnlySet<Guid>> GetLikedQuoteIdsAsync(
        Guid studentId, IEnumerable<Guid> teacherQuoteIds, CancellationToken ct = default);
    
    Task<IReadOnlyList<QuoteLikeStatDto>> GetQuoteLikeLeaderboardAsync(CancellationToken ct = default);
}