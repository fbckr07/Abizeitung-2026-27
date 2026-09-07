using Frontend.Data;
using Frontend.Data.DTOs;
using Frontend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frontend.Services;

public sealed class TeacherManagementService(AppDbContext db) : ITeacherManagementService
{
    public async Task<IReadOnlyList<TeacherListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var teacher = await db.Teacher
            .AsNoTracking()
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken);

        var referenzierteIds = await GetReferenzierteLehrerIdsAsync(cancellationToken);

        return teacher
            .Select(l => new TeacherListItemDto(l.Id, l.Name, l.Gender, !referenzierteIds.Contains(l.Id)))
            .ToList();
    }

    public async Task<Guid> CreateAsync(
        string name,
        Gender gender,
        CancellationToken cancellationToken = default)
    {
        ValidateName(name);

        var teacher = new Teacher
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Gender = gender,
        };

        db.Teacher.Add(teacher);
        await db.SaveChangesAsync(cancellationToken);
        return teacher.Id;
    }

    public async Task UpdateAsync(
        Guid id,
        string name,
        Gender gender,
        CancellationToken cancellationToken = default)
    {
        ValidateName(name);

        var teacher = await db.Teacher.FirstOrDefaultAsync(l => l.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Lehrer nicht gefunden.");

        teacher.Name = name.Trim();
        teacher.Gender = gender;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var referencedIds = await GetReferenzierteLehrerIdsAsync(cancellationToken);
        if (referencedIds.Contains(id))
        {
            throw new InvalidOperationException(
                "Dieser Lehrer kann nicht gelöscht werden, da noch Lehrerzitate oder Stimmen darauf verweisen.");
        }

        var teacher = await db.Teacher.FirstOrDefaultAsync(l => l.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Lehrer nicht gefunden.");

        db.Teacher.Remove(teacher);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<HashSet<Guid>> GetReferenzierteLehrerIdsAsync(CancellationToken cancellationToken)
    {
        var fromQuotes = await db.TeacherQuotes.Select(z => z.TeacherId).Distinct().ToListAsync(cancellationToken);
        var fromVotes = await db.TeacherVotes.Select(v => v.VotedTeacherId).Distinct().ToListAsync(cancellationToken);

        var all = new HashSet<Guid>(fromQuotes);
        all.UnionWith(fromVotes);
        return all;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Der Name darf nicht leer sein.");
        }
    }
}
