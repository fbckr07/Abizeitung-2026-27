using Frontend.Data.DTOs;

namespace Frontend.Services;

public interface IAdminUserService
{
    Task<IReadOnlyList<AdminUserDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<AdminUserDto> CreateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default);
}