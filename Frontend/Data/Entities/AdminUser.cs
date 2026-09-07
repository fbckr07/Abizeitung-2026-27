namespace Frontend.Data.Entities;

public sealed class AdminUser
{
    public Guid Id { get; set; }

    public required string Username { get; set; }

    public required string PasswordHash { get; set; }
}
