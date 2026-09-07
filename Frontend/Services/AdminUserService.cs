using Frontend.Data;
using Frontend.Data.DTOs;
using Frontend.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Frontend.Services;

public sealed class AdminUserService(AppDbContext db) : IAdminUserService
{
    private static readonly PasswordHasher<AdminUser> Hasher = new();

    public async Task<IReadOnlyList<AdminUserDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await db.AdminUsers
            .AsNoTracking()
            .OrderBy(admin => admin.Username)
            .Select(admin => new AdminUserDto(admin.Id, admin.Username))
            .ToListAsync(cancellationToken);
    }

    public async Task<AdminUserDto> CreateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        username = username.Trim();

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Der Benutzername darf nicht leer sein.", nameof(username));
        }

        if (username.Length > 100)
        {
            throw new ArgumentException("Der Benutzername darf höchstens 100 Zeichen enthalten.", nameof(username));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Das Passwort darf nicht leer sein.", nameof(password));
        }

        if (password.Length < 8)
        {
            throw new ArgumentException("Das Passwort muss mindestens 8 Zeichen enthalten.", nameof(password));
        }

        if (await db.AdminUsers.AnyAsync(admin => admin.Username == username, cancellationToken))
        {
            throw new InvalidOperationException("Dieser Benutzername ist bereits vergeben.");
        }

        var admin = new AdminUser
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = string.Empty
        };
        admin.PasswordHash = Hasher.HashPassword(admin, password);

        db.AdminUsers.Add(admin);
        await db.SaveChangesAsync(cancellationToken);

        return new AdminUserDto(admin.Id, admin.Username);
    }
}