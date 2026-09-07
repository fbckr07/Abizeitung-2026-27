using Frontend.Data.DTOs;

namespace Frontend.Services;


public interface IRankingService
{
    Task<IReadOnlyList<QuestionPairDto>> GetStudentQuestionsAsync(CancellationToken cancellationToken = default);
    
    Task CreateStudentQuestionPairAsync(
        string textMale,
        string textFemale,
        bool isDuo = false,
        bool isCrossGender = false,
        CancellationToken cancellationToken = default);

    Task UpdateStudentQuestionPairAsync(
        Guid questionGroupId,
        string textMale,
        string textFemale,
        bool isActive,
        DateTime? activeUntil,
        CancellationToken cancellationToken = default);
    
    Task DeleteStudentQuestionPairAsync(Guid questionGroupId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<QuestionPairDto>> GetTeacherQuestionsAsync(CancellationToken cancellationToken = default);

    Task CreateTeacherQuestionPairAsync(
        string textMale,
        string textFemale,
        CancellationToken cancellationToken = default);

    Task UpdateTeacherQuestionPairAsync(
        Guid questionGroupId,
        string textMale,
        string textFemale,
        bool isActive,
        DateTime? activeUntil,
        CancellationToken cancellationToken = default);
    
    Task DeleteTeacherQuestionPairAsync(Guid questionGroupId, CancellationToken cancellationToken = default);
}
