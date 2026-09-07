using Frontend.Data;
using Frontend.Data.DTOs;
using Frontend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frontend.Services;

public sealed class CommentService(AppDbContext db) : ICommentService
{
    public async Task<IReadOnlyList<CommentDto>> GetForStudentAsync(
        Guid targetId,
        CancellationToken cancellationToken = default)
    {
        return await db.Comments
            .AsNoTracking()
            .Where(k => k.TargetId == targetId && k.Confirmed)
            .Join(db.Students,
                comment => comment.AuthorId,
                author => author.Id,
                (comment, author) => new { comment = comment, author.Name })
            .OrderByDescending(k => k.comment.Date)
            .Select(k => new CommentDto(
                k.comment.Id,
                k.comment.Text,
                k.comment.AuthorId,
                k.Name,
                k.comment.TargetId,
                k.comment.Date,
                k.comment.Confirmed))
            .ToListAsync(cancellationToken);
    }

    public async Task<CommentDto> CreateAsync(
        Guid authorId,
        Guid targetId,
        string text,
        CancellationToken cancellationToken = default)
    {
        if (authorId == targetId)
        {
            throw new ArgumentException("Ein Schüler darf sich nicht selbst kommentieren.", nameof(targetId));
        }

        text = text.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Der Kommentar darf nicht leer sein.", nameof(text));
        }

        if (text.Length > 1024)
        {
            throw new ArgumentException("Der Kommentar darf höchstens 1024 Zeichen enthalten.", nameof(text));
        }

        var studentenExistieren = await db.Students
            .Where(s => s.Id == authorId || s.Id == targetId)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        if (!studentenExistieren.Contains(authorId))
        {
            throw new InvalidOperationException("Ersteller nicht gefunden.");
        }

        if (!studentenExistieren.Contains(targetId))
        {
            throw new InvalidOperationException("Ziel nicht gefunden.");
        }

        var kommentar = new Comment
        {
            Id = Guid.NewGuid(),
            Text = text,
            AuthorId = authorId,
            TargetId = targetId,
            Date = DateTime.UtcNow,
            Confirmed = false
        };

        db.Comments.Add(kommentar);
        await db.SaveChangesAsync(cancellationToken);

        var erstellerName = await db.Students
            .Where(s => s.Id == authorId)
            .Select(s => s.Name)
            .SingleAsync(cancellationToken);

        return new CommentDto(
            kommentar.Id,
            kommentar.Text,
            kommentar.AuthorId,
            erstellerName,
            kommentar.TargetId,
            kommentar.Date,
            kommentar.Confirmed);
    }

    public async Task ConfirmAsync(
        Guid commentId,
        Guid targetId,
        CancellationToken cancellationToken = default)
    {
        var comment = await db.Comments
            .FirstOrDefaultAsync(k => k.Id == commentId && k.TargetId == targetId, cancellationToken)
            ?? throw new InvalidOperationException("Kommentar nicht gefunden.");

        comment.Confirmed = true;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Guid commentId,
        Guid authorId,
        CancellationToken cancellationToken = default)
    {
        var comment = await db.Comments
            .FirstOrDefaultAsync(k => k.Id == commentId && k.AuthorId == authorId, cancellationToken)
            ?? throw new InvalidOperationException("Kommentar nicht gefunden.");

        db.Comments.Remove(comment);
        await db.SaveChangesAsync(cancellationToken);
    }
}