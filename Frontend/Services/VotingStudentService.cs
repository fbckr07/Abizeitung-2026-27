using Frontend.Data;
using Frontend.Data.Entities;
using Frontend.Data.Records;
using Microsoft.EntityFrameworkCore;

namespace Frontend.Services;

public sealed class VotingStudentService(AppDbContext db) : IVotingStudentService
{
    public async Task<IReadOnlyList<CategoryWithSelectionDto<Student>>> GetCategoriesWithOwnSelectionAsync(
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var categories = await db.StudentCategories
            .AsNoTracking()
            .Where(k => k.IsActive && (k.ActiveUntil == null || k.ActiveUntil > now))
            .OrderBy(k => k.QuestionText)
            .ToListAsync(cancellationToken);

        var categoryIds = categories.Select(k => k.Id).ToList();
        
        var ownVotes = await db.StudentVotes
            .AsNoTracking()
            .Where(v => v.VoterStudentId == studentId && categoryIds.Contains(v.CategoryStudentId))
            .ToDictionaryAsync(v => v.CategoryStudentId, v => (VotedStudentId: v.VotedStudentId, VotedStudent2Id: v.VotedStudent2Id), cancellationToken);
        
        var selectedIds = ownVotes.Values
            .SelectMany(v => v.VotedStudent2Id is null
                ? new[] { v.VotedStudentId }
                : new[] { v.VotedStudentId, v.VotedStudent2Id.Value })
            .Distinct()
            .ToList();
        var votedStudents = await db.Students
            .AsNoTracking()
            .Where(s => selectedIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        return categories
            .Select(k =>
            {
                ownVotes.TryGetValue(k.Id, out var selection);
                votedStudents.TryGetValue(selection.VotedStudentId, out var first);
                Student? second = selection.VotedStudent2Id is { } secondId && votedStudents.TryGetValue(secondId, out var s)
                    ? s
                    : null;

                return new CategoryWithSelectionDto<Student>
                {
                    CategoryId = k.Id,
                    QuestionGroupId = k.QuestionGroupId,
                    Gender = k.Gender,
                    QuestionText = k.QuestionText,
                    IsDuo = k.IsDuo,
                    IsCrossGender = k.IsCrossGender,
                    PreviousSelection = first,
                    PreviousSelection2 = second
                };
            })
            .ToList();
    }

    public async Task<IReadOnlyList<CandidateDto>> GetCandidatesForCategoryAsync(
        Guid categoryId,
        Guid voterId,
        CancellationToken cancellationToken = default)
    {
        var gender = await db.StudentCategories
            .AsNoTracking()
            .Where(k => k.Id == categoryId)
            .Select(k => new { Gender = k.Gender, IsCrossGender = k.IsCrossGender })
            .FirstOrDefaultAsync(cancellationToken);

        if (gender is null)
        {
            return [];
        }

        return await db.Students
            .AsNoTracking()
            .Where(s => (gender.IsCrossGender || s.Gender == gender.Gender) && s.Id != voterId)
            .OrderBy(s => s.Name)
            .Select(s => new CandidateDto(s.Id, s.Name, s.Course))
            .ToListAsync(cancellationToken);
    }

    public async Task UpsertVoteAsync(
        Guid categoryId,
        Guid voterId,
        Guid votedStudentId,
        CancellationToken cancellationToken = default)
    {
        if (voterId == votedStudentId)
        {
            throw new InvalidOperationException("Du kannst nicht für dich selbst abstimmen.");
        }

        var now = DateTime.UtcNow;

        var category = await db.StudentCategories
            .FirstOrDefaultAsync(k => k.Id == categoryId, cancellationToken)
            ?? throw new InvalidOperationException("Kategorie nicht gefunden.");

        if (!category.IsActive || (category.ActiveUntil is not null && category.ActiveUntil <= now))
        {
            throw new InvalidOperationException("Diese Kategorie ist nicht mehr aktiv.");
        }

        if (category.IsDuo)
        {
            throw new InvalidOperationException("Diese Kategorie ist eine Duo-Frage.");
        }

        var voted = await db.Students
            .FirstOrDefaultAsync(s => s.Id == votedStudentId, cancellationToken)
            ?? throw new InvalidOperationException("Gewählter Student nicht gefunden.");

        if (voted.Gender != category.Gender)
        {
            throw new InvalidOperationException("Der gewählte Student passt nicht zum Geschlecht der Kategorie.");
        }
        
        const string sql = """
            INSERT INTO "StudentVotes" ("Id", "CategoryStudentId", "VoterStudentId", "VotedStudentId", "CreatedAt")
            VALUES ({0}, {1}, {2}, {3}, {4})
            ON CONFLICT ("CategoryStudentId", "VoterStudentId")
            DO UPDATE SET "VotedStudentId" = EXCLUDED."VotedStudentId", "VotedStudent2Id" = NULL
            """;

        await db.Database.ExecuteSqlRawAsync(
            sql,
            Guid.NewGuid(),
            categoryId,
            voterId,
            votedStudentId,
            now);
    }

    public async Task UpsertDuoVoteAsync(
        Guid categoryId,
        Guid voterId,
        Guid votedStudent1,
        Guid votedStudent2,
        CancellationToken cancellationToken = default)
    {
        if (votedStudent1 == votedStudent2)
        {
            throw new InvalidOperationException("Bitte zwei unterschiedliche Personen auswählen.");
        }

        if (voterId == votedStudent1 || voterId == votedStudent2)
        {
            throw new InvalidOperationException("Du kannst nicht für dich selbst abstimmen.");
        }

        var now = DateTime.UtcNow;

        var category = await db.StudentCategories
            .FirstOrDefaultAsync(k => k.Id == categoryId, cancellationToken)
            ?? throw new InvalidOperationException("Kategorie nicht gefunden.");

        if (!category.IsActive || (category.ActiveUntil is not null && category.ActiveUntil <= now))
        {
            throw new InvalidOperationException("Diese Kategorie ist nicht mehr aktiv.");
        }

        if (!category.IsDuo)
        {
            throw new InvalidOperationException("Diese Kategorie ist keine Duo-Frage.");
        }

        var voted = await db.Students
            .Where(s => s.Id == votedStudent1 || s.Id == votedStudent2)
            .ToListAsync(cancellationToken);

        if (voted.Count != 2)
        {
            throw new InvalidOperationException("Gewählte Studenten nicht gefunden.");
        }

        if (!category.IsCrossGender && voted.Any(s => s.Gender != category.Gender))
        {
            throw new InvalidOperationException("Die gewählten Studenten passen nicht zum Geschlecht der Kategorie.");
        }
        
        var (firstId, secondId) = votedStudent1.CompareTo(votedStudent2) <= 0
            ? (votedStudent1: votedStudent1, votedStudent2: votedStudent2)
            : (votedStudent1: votedStudent2, votedStudent2: votedStudent1);
        
        const string sql = """
            INSERT INTO "StudentVotes" ("Id", "CategoryStudentId", "VoterStudentId", "VotedStudentId", "VotedStudent2Id", "CreatedAt")
            VALUES ({0}, {1}, {2}, {3}, {4}, {5})
            ON CONFLICT ("CategoryStudentId", "VoterStudentId")
            DO UPDATE SET "VotedStudentId" = EXCLUDED."VotedStudentId", "VotedStudent2Id" = EXCLUDED."VotedStudent2Id"
            """;

        await db.Database.ExecuteSqlRawAsync(
            sql,
            Guid.NewGuid(),
            categoryId,
            voterId,
            firstId,
            secondId,
            now);
    }
}
