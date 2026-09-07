using Frontend.Data.Entities;

namespace Frontend.Services;

public interface IFeedbackService
{
    Task<Feedback> CreateAsync(string text, string sentBy, FeedbackType type,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Feedback>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> AcceptFeedbackAsync(Guid feedbackId, CancellationToken cancellationToken = default);
    Task<bool> RejectFeedbackAsync(Guid feedbackId, CancellationToken cancellationToken = default);
}