namespace Domain.Entities;

public record User
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Email { get; set; }

    public string? PasswordHash { get; set; }

    public DateTime CreateDate { get; } = DateTime.UtcNow;

    public bool HasGoogleAuth { get; set; }

    public GoogleUser? GoogleUser { get; set; }
}
