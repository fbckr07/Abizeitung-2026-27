using Frontend.Data.Entities;

namespace Frontend.Services;

public sealed record ResultEntryDto(int Place, string Name, int Votes);

public sealed record ResultCategoryDto(
    Guid QuestionGroupId,
    Gender Gender,
    string QuestionText,
    IReadOnlyList<ResultEntryDto> TopThree,
    bool IsCrossGender = false);

public interface IResultService
{
    Task<IReadOnlyList<ResultCategoryDto>> GetStudentResultsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ResultCategoryDto>> GetTeacherResultsAsync(CancellationToken cancellationToken = default);
    
    Task<string> ExportStudentCsvAsync(CancellationToken cancellationToken = default);

    Task<string> ExportTeacherCsvAsync(CancellationToken cancellationToken = default);
}
