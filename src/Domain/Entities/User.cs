namespace Domain.Entities;

public record User
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Email { get; set; }

    public string? PasswordHash { get; set; }

    public bool HasGoogleAuth { get; set; }

    public DateTime CreateDate { get; set; } = DateTime.UtcNow;

    public GoogleUser GoogleUser { get; set; }
}