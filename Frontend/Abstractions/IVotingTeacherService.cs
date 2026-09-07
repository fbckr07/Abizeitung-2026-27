using Frontend.Data.Entities;
using Frontend.Data.Records;

namespace Frontend.Services;

public interface IVotingTeacherService
{
    Task<IReadOnlyList<CategoryWithSelectionDto<Teacher>>> GetCategoriesWithOwnSelectionAsync(
        Guid studentId,
        CancellationToken cancellationToken = default);
    
    Task UpsertVoteAsync(
        Guid categoryId,
        Guid voterId,
        Guid votedTeacherId,
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<CandidateDto>> GetCandidatesForCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);
}
