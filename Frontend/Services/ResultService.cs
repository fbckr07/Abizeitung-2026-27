using System.Globalization;
using System.Text;
using Frontend.Data;
using Microsoft.EntityFrameworkCore;

namespace Frontend.Services;

public sealed class ResultService(AppDbContext db) : IResultService
{
    private const int TopN = 3;

    public async Task<IReadOnlyList<ResultCategoryDto>> GetStudentResultsAsync(
        CancellationToken cancellationToken = default)
    {
        var categories = await db.StudentCategories
            .AsNoTracking()
            .OrderBy(k => k.QuestionText)
            .ToListAsync(cancellationToken);

        var results = new List<ResultCategoryDto>();

        foreach (var category in categories)
        {
            if (category.IsDuo)
            {
                var topDuos = await db.StudentVotes
                    .AsNoTracking()
                    .Where(v => v.CategoryStudentId == category.Id && v.VotedStudent2Id != null)
                    .GroupBy(v => new { VotedStudentId = v.VotedStudentId, VotedStudentId2 = v.VotedStudent2Id })
                    .Select(g => new { VotedStudentId = g.Key.VotedStudentId, VotedStudentId2 = g.Key.VotedStudentId2!.Value, Votes = g.Count() })
                    .OrderByDescending(g => g.Votes)
                    .Take(TopN)
                    .ToListAsync(cancellationToken);

                var duoStudentIds = topDuos.SelectMany(t => new[] { t.VotedStudentId, t.VotedStudentId2 }).Distinct().ToList();
                var duoNames = await db.Students
                    .AsNoTracking()
                    .Where(s => duoStudentIds.Contains(s.Id))
                    .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);

                var topThreeDuos = topDuos
                    .Select((t, index) => new ResultEntryDto(
                        index + 1,
                        $"{duoNames.GetValueOrDefault(t.VotedStudentId, "Unbekannt")} & {duoNames.GetValueOrDefault(t.VotedStudentId2, "Unbekannt")}",
                        t.Votes))
                    .ToList();

                results.Add(new ResultCategoryDto(category.QuestionGroupId, category.Gender, category.QuestionText, topThreeDuos, category.IsCrossGender));
                continue;
            }

            var topVotes = await db.StudentVotes
                .AsNoTracking()
                .Where(v => v.CategoryStudentId == category.Id)
                .GroupBy(v => v.VotedStudentId)
                .Select(g => new { StudentId = g.Key, Votes = g.Count() })
                .OrderByDescending(g => g.Votes)
                .Take(TopN)
                .ToListAsync(cancellationToken);

            var studentIds = topVotes.Select(t => t.StudentId).ToList();
            var names = await db.Students
                .AsNoTracking()
                .Where(s => studentIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);

            var topThree = topVotes
                .Select((t, index) => new ResultEntryDto(index + 1, names.GetValueOrDefault(t.StudentId, "Unbekannt"), t.Votes))
                .ToList();

            results.Add(new ResultCategoryDto(category.QuestionGroupId, category.Gender, category.QuestionText, topThree));
        }

        return results;
    }

    public async Task<IReadOnlyList<ResultCategoryDto>> GetTeacherResultsAsync(
        CancellationToken cancellationToken = default)
    {
        var categories = await db.TeacherCategories
            .AsNoTracking()
            .OrderBy(k => k.QuestionText)
            .ToListAsync(cancellationToken);

        var results = new List<ResultCategoryDto>();

        foreach (var category in categories)
        {
            var topVotes = await db.TeacherVotes
                .AsNoTracking()
                .Where(v => v.CategoryTeacherId == category.Id)
                .GroupBy(v => v.VotedTeacherId)
                .Select(g => new { TeacherId = g.Key, Votes = g.Count() })
                .OrderByDescending(g => g.Votes)
                .Take(TopN)
                .ToListAsync(cancellationToken);

            var teacherIds = topVotes.Select(t => t.TeacherId).ToList();
            var names = await db.Teacher
                .AsNoTracking()
                .Where(l => teacherIds.Contains(l.Id))
                .ToDictionaryAsync(l => l.Id, l => l.Name, cancellationToken);

            var topThree = topVotes
                .Select((t, index) => new ResultEntryDto(index + 1, names.GetValueOrDefault(t.TeacherId, "Unbekannt"), t.Votes))
                .ToList();

            results.Add(new ResultCategoryDto(category.QuestionGroupId, category.Gender, category.QuestionText, topThree));
        }

        return results;
    }

    public async Task<string> ExportStudentCsvAsync(CancellationToken cancellationToken = default)
    {
        return BuildCsv(await GetStudentResultsAsync(cancellationToken));
    }

    public async Task<string> ExportTeacherCsvAsync(CancellationToken cancellationToken = default)
    {
        return BuildCsv(await GetTeacherResultsAsync(cancellationToken));
    }

    private static string BuildCsv(IReadOnlyList<ResultCategoryDto> results)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Fragengruppe,Geschlecht-Variante,Frage,Platz,Name,Stimmenanzahl");

        foreach (var category in results)
        {
            foreach (var entry in category.TopThree)
            {
                sb.AppendLine(string.Join(",",
                    CsvEscape(category.QuestionGroupId.ToString()),
                    CsvEscape(category.IsCrossGender ? "Beide" : category.Gender.ToString()),
                    CsvEscape(category.QuestionText),
                    entry.Place.ToString(CultureInfo.InvariantCulture),
                    CsvEscape(entry.Name),
                    entry.Votes.ToString(CultureInfo.InvariantCulture)));
            }
        }

        return sb.ToString();
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }
}
