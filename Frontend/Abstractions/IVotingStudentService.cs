using Frontend.Data.Entities;
using Frontend.Data.Records;

namespace Frontend.Services;

public interface IVotingStudentService
{
    Task<IReadOnlyList<CategoryWithSelectionDto<Student>>> GetCategoriesWithOwnSelectionAsync(
        Guid studentId,
        CancellationToken cancellationToken = default);
    
    Task UpsertVoteAsync(
        Guid categoryId,
        Guid voterId,
        Guid votedStudentId,
        CancellationToken cancellationToken = default);
    
    Task UpsertDuoVoteAsync(
        Guid categoryId,
        Guid voterId,
        Guid votedStudent1,
        Guid votedStudent2,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CandidateDto>> GetCandidatesForCategoryAsync(
        Guid categoryId,
        Guid voterId,
        CancellationToken cancellationToken = default);
}
