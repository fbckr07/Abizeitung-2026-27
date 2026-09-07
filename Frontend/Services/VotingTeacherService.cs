using Frontend.Data;
using Frontend.Data.Entities;
using Frontend.Data.Records;
using Microsoft.EntityFrameworkCore;

namespace Frontend.Services;

public sealed class VotingTeacherService(AppDbContext db) : IVotingTeacherService
{
    public async Task<IReadOnlyList<CategoryWithSelectionDto<Teacher>>> GetCategoriesWithOwnSelectionAsync(
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var categories = await db.TeacherCategories
            .AsNoTracking()
            .Where(k => k.IsActive && (k.ActiveUntil == null || k.ActiveUntil > now))
            .OrderBy(k => k.QuestionText)
            .ToListAsync(cancellationToken);

        var categoryIds = categories.Select(k => k.Id).ToList();
        
        var ownVotes = await db.TeacherVotes
            .AsNoTracking()
            .Where(v => v.VoterStudentId == studentId && categoryIds.Contains(v.CategoryTeacherId))
            .ToDictionaryAsync(v => v.CategoryTeacherId, v => v.VotedTeacherId, cancellationToken);
        
        var votedIds = ownVotes.Values.Distinct().ToList();
        var votedTeacher = await db.Teacher
            .AsNoTracking()
            .Where(l => votedIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, cancellationToken);

        return categories
            .Select(k => new CategoryWithSelectionDto<Teacher>
            {
                CategoryId = k.Id,
                QuestionGroupId = k.QuestionGroupId,
                Gender = k.Gender,
                QuestionText = k.QuestionText,
                PreviousSelection = ownVotes.TryGetValue(k.Id, out var votedId)
                    && votedTeacher.TryGetValue(votedId, out var teacher)
                        ? teacher
                        : null
            })
            .ToList();
    }

    public async Task<IReadOnlyList<CandidateDto>> GetCandidatesForCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        var gender = await db.TeacherCategories
            .AsNoTracking()
            .Where(k => k.Id == categoryId)
            .Select(k => (Gender?)k.Gender)
            .FirstOrDefaultAsync(cancellationToken);

        if (gender is null)
        {
            return [];
        }

        return await db.Teacher
            .AsNoTracking()
            .Where(l => l.Gender == gender)
            .OrderBy(l => l.Name)
            .Select(l => new CandidateDto(l.Id, l.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task UpsertVoteAsync(
        Guid categoryId,
        Guid voterId,
        Guid votedTeacherId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var category = await db.TeacherCategories
            .FirstOrDefaultAsync(k => k.Id == categoryId, cancellationToken)
            ?? throw new InvalidOperationException("Kategorie nicht gefunden.");

        if (!category.IsActive || (category.ActiveUntil is not null && category.ActiveUntil <= now))
        {
            throw new InvalidOperationException("Diese Kategorie ist nicht mehr aktiv.");
        }

        var voted = await db.Teacher
            .FirstOrDefaultAsync(l => l.Id == votedTeacherId, cancellationToken)
            ?? throw new InvalidOperationException("Gewählter Lehrer nicht gefunden.");

        if (voted.Gender != category.Gender)
        {
            throw new InvalidOperationException("Der gewählte Lehrer passt nicht zum Geschlecht der Kategorie.");
        }
        
        const string sql = """
            INSERT INTO "TeacherVotes" ("Id", "CategoryTeacherId", "VoterStudentId", "VotedTeacherId", "CreatedAt")
            VALUES ({0}, {1}, {2}, {3}, {4})
            ON CONFLICT ("CategoryTeacherId", "VoterStudentId")
            DO UPDATE SET "VotedTeacherId" = EXCLUDED."VotedTeacherId"
            """;

        await db.Database.ExecuteSqlRawAsync(
            sql,
            Guid.NewGuid(),
            categoryId,
            voterId,
            votedTeacherId,
            now);
    }
}
