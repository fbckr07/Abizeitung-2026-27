using Frontend.Data.DTOs;

namespace Frontend.Services;


public interface ICommentService
{
    Task<IReadOnlyList<CommentDto>> GetForStudentAsync(
        Guid targetId,
        CancellationToken cancellationToken = default);

    Task<CommentDto> CreateAsync(
        Guid authorId,
        Guid targetId,
        string text,
        CancellationToken cancellationToken = default);

    Task ConfirmAsync(
        Guid commentId,
        Guid targetId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid commentId,
        Guid authorId,
        CancellationToken cancellationToken = default);
}