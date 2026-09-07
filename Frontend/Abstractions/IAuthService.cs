using Frontend.Data.Entities;

namespace Frontend.Services;

public interface IAuthService
{
    Task<Student?> ValidateCodeAsync(string code, CancellationToken cancellationToken = default);
}
