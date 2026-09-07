using Frontend.Data.Entities;
using Frontend.Data.Records;

namespace Frontend.Services;

public interface ITeacherQuoteService
{
    Task<IReadOnlyList<CandidateDto>> GetTeacherAsync(CancellationToken cancellationToken = default);
    
    Task SubmitAsync(
        Guid teacherId,
        string quote,
        string? context,
        Guid? submittedByStudent,
        CancellationToken cancellationToken = default);
    
    Task<TeacherQuotePage> GetAcceptedQuotesAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
