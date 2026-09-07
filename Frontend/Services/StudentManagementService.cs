using Frontend.Data;
using Frontend.Data.DTOs;
using Frontend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frontend.Services;

public sealed class StudentManagementService(AppDbContext db) : IStudentManagementService
{
    private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public async Task<Student?> GetStudentAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await db.Students.FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken);
    }

    public async Task<IReadOnlyList<StudentListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await db.Students
            .OrderBy(s => s.Name)
            .Select(s => new StudentListItemDto(s.Id, s.Name, s.Course ?? "", s.LoginCode, s.Gender,
                s.Permissions ?? new List<string>()))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StudentOverviewDto>> GetOverviewAsync(
        Guid ownStudentId,
        CancellationToken cancellationToken = default)
    {
        return await db.Students
            .AsNoTracking()
            .OrderByDescending(s => s.Id == ownStudentId)
            .ThenBy(s => s.Name)
            .Select(s => new StudentOverviewDto(
                s.Id,
                s.Name,
                s.Course ?? "",
                s.Gender,
                s.Id == ownStudentId))
            .ToListAsync(cancellationToken);
    }

    public async Task<StudentListItemDto> CreateAsync(string name, string course, Gender gender,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name darf nicht leer sein.", nameof(name));
        }

        var loginCode = await GenerateUniqueLoginCodeAsync(cancellationToken);

        var student = new Student
        {
            Name = name.Trim(),
            Course = course.Trim(),
            Gender = gender,
            LoginCode = loginCode,
            CreatedAt = DateTime.UtcNow,
            Permissions = (List<string>)[Permissions.Lehrerzitate, Permissions.RankingSchueler]
        };

        db.Students.Add(student);
        await db.SaveChangesAsync(cancellationToken);

        return ToListItem(student);
    }

    public async Task<StudentListItemDto> CreateAsync(Student student, CancellationToken cancellationToken = default)
    {
        student.Permissions = new List<string>([Permissions.Lehrerzitate, Permissions.RankingSchueler]);

        db.Students.Add(student);
        await db.SaveChangesAsync(cancellationToken);

        return ToListItem(student);
    }

    public async Task<StudentListItemDto> CreateAsync(string name, string course, Gender gender, string loginCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name darf nicht leer sein.", nameof(name));
        }

        loginCode = loginCode.Trim();
        if (string.IsNullOrWhiteSpace(loginCode))
        {
            throw new ArgumentException("Login-Code darf nicht leer sein.", nameof(loginCode));
        }

        if (await db.Students.AnyAsync(s => s.LoginCode == loginCode, cancellationToken))
        {
            throw new InvalidOperationException($"Login-Code {loginCode} ist bereits vergeben.");
        }

        var student = new Student
        {
            Name = name.Trim(),
            Course = course.Trim(),
            Gender = gender,
            LoginCode = loginCode,
            CreatedAt = DateTime.UtcNow,
            Permissions = (List<string>)[Permissions.Lehrerzitate, Permissions.RankingSchueler]
        };
        
        db.Students.Add(student);
        await db.SaveChangesAsync(cancellationToken);
        
        return ToListItem(student);
    }

    public async Task SetPermissionAsync(Guid studentId, IReadOnlyList<string> permissions, CancellationToken cancellationToken = default)
    {
        var student = await db.Students.FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken)
            ?? throw new InvalidOperationException("Student nicht gefunden.");

        student.Permissions = permissions.Distinct().ToList();
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AddPermissionForAllAsync(string permission, CancellationToken cancellationToken = default)
    {
        if (!Permissions.Alle.Any(item => item.Value == permission))
        {
            throw new ArgumentException("Unbekanntes Recht.", nameof(permission));
        }

        var students = await db.Students.ToListAsync(cancellationToken);
        foreach (var student in students)
        {
            student.Permissions ??= [];
            if (!student.Permissions.Contains(permission))
            {
                student.Permissions.Add(permission);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SetGenderAsync(Guid studentId, Gender gender,
        CancellationToken cancellationToken = default)
    {
        var student = await db.Students.FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken)
            ?? throw new InvalidOperationException("Student nicht gefunden.");

        student.Gender = gender;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SetCourseAsync(Guid studentId, string course, CancellationToken cancellationToken = default)
    {
        var student = await db.Students.FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken)
            ?? throw new InvalidOperationException("Student nicht gefunden.");

        student.Course = course.Trim();
        await db.SaveChangesAsync(cancellationToken);
    }

    private static StudentListItemDto ToListItem(Student student) =>
        new(student.Id, student.Name, student.Course ?? "", student.LoginCode, student.Gender, student.Permissions ?? []);

    private async Task<string> GenerateUniqueLoginCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            var code = GenerateLoginCode();
            var existing = await db.Students.AnyAsync(s => s.LoginCode == code, cancellationToken);
            if (!existing)
            {
                return code;
            }
        }

        throw new InvalidOperationException("Konnte keinen eindeutigen Login-Code generieren.");
    }

    private static string GenerateLoginCode()
    {
        Span<char> chars = stackalloc char[6];
        for (var i = 0; i < chars.Length; i++)
        {
            chars[i] = Random.Shared.Next(0, 10).ToString()[0];
        }

        return new string(chars);
    }
}
