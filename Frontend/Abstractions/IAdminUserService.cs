using Frontend.Data.DTOs;

namespace Frontend.Services;

public interface IAdminUserService
{
    Task<IReadOnlyList<AdminUserDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<AdminUserDto> CreateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid adminId, CancellationToken cancellationToken = default);

    Task SetPermissionAsync(Guid adminId, IReadOnlyList<string> permissions,
        CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(Guid adminId, string newPassword, CancellationToken cancellationToken = default);
}