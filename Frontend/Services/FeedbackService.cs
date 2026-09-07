using Frontend.Data;
using Frontend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frontend.Services;

public class FeedbackService(AppDbContext db) : IFeedbackService
{
    public async Task<Feedback> CreateAsync(string text, string sentBy, FeedbackType type,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(text))
            throw new ArgumentException("Feedback text cannot be empty.", nameof(text));
        var feedback = new Feedback
        {
            Text = text,
            SentBy = sentBy,
            Type = type,
            Archived = false,
            CreatedAt = DateTime.UtcNow
        };

        db.Feedbacks.Add(feedback);
        await db.SaveChangesAsync(cancellationToken);

        return feedback;
    }

    public async Task<IReadOnlyList<Feedback>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await db.Feedbacks.ToListAsync(cancellationToken);
    }
    
    public async Task<bool> AcceptFeedbackAsync(Guid feedbackId, CancellationToken cancellationToken = default)
    {
        var feedback = await db.Feedbacks.FirstOrDefaultAsync(f => f.Id == feedbackId, cancellationToken);
        if (feedback == null)
            return false;

        feedback.Archived = true;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RejectFeedbackAsync(Guid feedbackId, CancellationToken cancellationToken = default)
    {
        var feedback = await db.Feedbacks.FirstOrDefaultAsync(f => f.Id == feedbackId, cancellationToken);
        if (feedback == null)
            return false;

        db.Feedbacks.Remove(feedback);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
 }