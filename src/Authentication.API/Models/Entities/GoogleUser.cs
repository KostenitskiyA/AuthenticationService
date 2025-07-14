namespace Authentication.API.Models.Entities;

public class GoogleUser
{
    public Guid Id { get; set; }
    
    public required string GoogleId { get; set; }
    
    public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    
    public User User { get; set; }
}