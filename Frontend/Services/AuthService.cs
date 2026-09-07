using Frontend.Data;
using Frontend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frontend.Services;

public sealed class AuthService(AppDbContext db) : IAuthService
{
    public async Task<Student?> ValidateCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var normalized = code.Trim().ToUpperInvariant();

        return await db.Students
            .FirstOrDefaultAsync(s => s.LoginCode.ToUpper() == normalized, cancellationToken);
    }
}
