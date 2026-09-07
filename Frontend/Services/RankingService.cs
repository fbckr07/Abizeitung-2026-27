using Frontend.Data;
using Frontend.Data.DTOs;
using Frontend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frontend.Services;

public sealed class RankingService(AppDbContext db) : IRankingService
{
    public async Task<IReadOnlyList<QuestionPairDto>> GetStudentQuestionsAsync(CancellationToken cancellationToken = default)
    {
        var categories = await db.StudentCategories.AsNoTracking().ToListAsync(cancellationToken);
        var votedGroups = await GetGroupsWithVotes(
            db.StudentVotes.Select(v => v.StudentCategory.QuestionGroupId),
            cancellationToken);

        return ToPairGroups(categories, k => k.QuestionGroupId, k => k.Gender, k => k.QuestionText, k => k.IsActive, k => k.ActiveUntil, k => k.Id, votedGroups, k => k.IsDuo);
    }

    public async Task CreateStudentQuestionPairAsync(
        string textMale,
        string textFemale,
        bool isDuo = false,
        bool isCrossGender = false,
        CancellationToken cancellationToken = default)
    {
        if (!isDuo && isCrossGender)
        {
            throw new InvalidOperationException("Eine geschlechtsübergreifende Frage muss eine Duo-Frage sein.");
        }

        ValidateTexte(textMale, textFemale, isCrossGender);
        var groupId = Guid.NewGuid();

        if (isCrossGender)
        {
            db.StudentCategories.Add(new StudentCategory
            {
                Id = Guid.NewGuid(),
                QuestionGroupId = groupId,
                Gender = Gender.Male,
                QuestionText = textMale.Trim(),
                IsActive = true,
                IsDuo = true,
                IsCrossGender = true
            });
        }
        else
        {
            db.StudentCategories.AddRange(
                new StudentCategory
                {
                    Id = Guid.NewGuid(),
                    QuestionGroupId = groupId,
                    Gender = Gender.Male,
                    QuestionText = textMale.Trim(),
                    IsActive = true,
                    IsDuo = isDuo
                },
                new StudentCategory
                {
                    Id = Guid.NewGuid(),
                    QuestionGroupId = groupId,
                    Gender = Gender.Female,
                    QuestionText = textFemale.Trim(),
                    IsActive = true,
                    IsDuo = isDuo
                });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateStudentQuestionPairAsync(
        Guid questionGroupId,
        string textMale,
        string textFemale,
        bool isActive,
        DateTime? activeUntil,
        CancellationToken cancellationToken = default)
    {
        var categories = await db.StudentCategories
            .Where(k => k.QuestionGroupId == questionGroupId)
            .ToListAsync(cancellationToken);

        if (categories.Count == 0)
        {
            throw new InvalidOperationException("Fragen-Paar nicht gefunden.");
        }

        var isCrossGender = categories.Count == 1 && categories[0].IsCrossGender;
        ValidateTexte(textMale, textFemale, isCrossGender);

        foreach (var category in categories)
        {
            category.QuestionText = category.Gender == Gender.Male ? textMale.Trim() : textFemale.Trim();
            category.IsActive = isActive;
            category.ActiveUntil = activeUntil;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteStudentQuestionPairAsync(Guid questionGroupId, CancellationToken cancellationToken = default)
    {
        var categories = await db.StudentCategories
            .Where(k => k.QuestionGroupId == questionGroupId)
            .ToListAsync(cancellationToken);

        if (categories.Count == 0)
        {
            return;
        }

        var categoryIds = categories.Select(k => k.Id).ToList();
        var hasVotes = await db.StudentVotes.AnyAsync(v => categoryIds.Contains(v.CategoryStudentId), cancellationToken);

        if (hasVotes)
        {
            throw new InvalidOperationException(
                "Zu diesem Fragen-Paar existieren bereits Stimmen. Bitte deaktivieren statt löschen.");
        }

        db.StudentCategories.RemoveRange(categories);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<QuestionPairDto>> GetTeacherQuestionsAsync(CancellationToken cancellationToken = default)
    {
        var categories = await db.TeacherCategories.AsNoTracking().ToListAsync(cancellationToken);
        var votedGroups = await GetGroupsWithVotes(
            db.TeacherVotes.Select(v => v.TeacherCategory.QuestionGroupId),
            cancellationToken);

        return ToPairGroups(categories, k => k.QuestionGroupId, k => k.Gender, k => k.QuestionText, k => k.IsActive, k => k.ActiveUntil, k => k.Id, votedGroups, _ => false);
    }

    public async Task CreateTeacherQuestionPairAsync(
        string textMale,
        string textFemale,
        CancellationToken cancellationToken = default)
    {
        ValidateTexte(textMale, textFemale);
        var groupId = Guid.NewGuid();

        db.TeacherCategories.AddRange(
            new TeacherCategory
            {
                Id = Guid.NewGuid(),
                QuestionGroupId = groupId,
                Gender = Gender.Male,
                QuestionText = textMale.Trim(),
                IsActive = true
            },
            new TeacherCategory
            {
                Id = Guid.NewGuid(),
                QuestionGroupId = groupId,
                Gender = Gender.Female,
                QuestionText = textFemale.Trim(),
                IsActive = true
            });

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateTeacherQuestionPairAsync(
        Guid questionGroupId,
        string textMale,
        string textFemale,
        bool isActive,
        DateTime? activeUntil,
        CancellationToken cancellationToken = default)
    {
        ValidateTexte(textMale, textFemale);

        var categories = await db.TeacherCategories
            .Where(k => k.QuestionGroupId == questionGroupId)
            .ToListAsync(cancellationToken);

        if (categories.Count == 0)
        {
            throw new InvalidOperationException("Fragen-Paar nicht gefunden.");
        }

        foreach (var category in categories)
        {
            category.QuestionText = category.Gender == Gender.Male ? textMale.Trim() : textFemale.Trim();
            category.IsActive = isActive;
            category.ActiveUntil = activeUntil;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteTeacherQuestionPairAsync(Guid questionGroupId, CancellationToken cancellationToken = default)
    {
        var categories = await db.TeacherCategories
            .Where(k => k.QuestionGroupId == questionGroupId)
            .ToListAsync(cancellationToken);

        if (categories.Count == 0)
        {
            return;
        }

        var categoryIds = categories.Select(k => k.Id).ToList();
        var hasVotes = await db.TeacherVotes.AnyAsync(v => categoryIds.Contains(v.CategoryTeacherId), cancellationToken);

        if (hasVotes)
        {
            throw new InvalidOperationException(
                "Zu diesem Fragen-Paar existieren bereits Stimmen. Bitte stattdessen deaktivieren statt löschen.");
        }

        db.TeacherCategories.RemoveRange(categories);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task<HashSet<Guid>> GetGroupsWithVotes(
        IQueryable<Guid> gruppenIdsQuery,
        CancellationToken cancellationToken)
    {
        var ids = await gruppenIdsQuery.Distinct().ToListAsync(cancellationToken);
        return [.. ids];
    }

    private static void ValidateTexte(string textMale, string textFemale, bool crossGender = false)
    {
        if (string.IsNullOrWhiteSpace(textMale) || (!crossGender && string.IsNullOrWhiteSpace(textFemale)))
        {
            throw new InvalidOperationException(crossGender
                ? "Der Fragetext muss ausgefüllt werden."
                : "Beide Textvarianten (männlich und weiblich) müssen ausgefüllt werden.");
        }
    }

    private static List<QuestionPairDto> ToPairGroups<TKategorie>(
        List<TKategorie> categories,
        Func<TKategorie, Guid> groupIdSelector,
        Func<TKategorie, Gender> genderSelector,
        Func<TKategorie, string> textSelector,
        Func<TKategorie, bool> isActiveSelector,
        Func<TKategorie, DateTime?> activeUntilSelector,
        Func<TKategorie, Guid> idSelector,
        HashSet<Guid> groupWithVotesSelector,
        Func<TKategorie, bool> isDuoSelector)
    {
        return categories
            .GroupBy(groupIdSelector)
            .Select(gruppe =>
            {
                var male = gruppe.FirstOrDefault(k => genderSelector(k) == Gender.Male);
                var female = gruppe.FirstOrDefault(k => genderSelector(k) == Gender.Female);

                return new QuestionPairDto(
                    gruppe.Key,
                    male is not null ? idSelector(male) : Guid.Empty,
                    female is not null ? idSelector(female) : Guid.Empty,
                    male is not null ? textSelector(male) : string.Empty,
                    female is not null ? textSelector(female) : string.Empty,
                    male is not null && isActiveSelector(male),
                    male is not null ? activeUntilSelector(male) : null,
                    groupWithVotesSelector.Contains(gruppe.Key),
                    male is not null && isDuoSelector(male),
                    gruppe.Any(k => k is StudentCategory student && student.IsCrossGender));
            })
            .OrderBy(p => p.TextMale)
            .ToList();
    }
}
