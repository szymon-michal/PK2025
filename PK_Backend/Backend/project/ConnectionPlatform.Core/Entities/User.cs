namespace ConnectionPlatform.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Nick { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public int? Age { get; set; }
    public bool IsActive { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? FcmToken { get; set; }

    // Navigation properties
    public UserProfilePhoto? ProfilePhoto { get; set; }
    public UserInterests? Interests { get; set; }
    public UserFriends? Friends { get; set; }
    public UserBlocked? BlockedUsers { get; set; }
    public ICollection<Repository> Repositories { get; set; } = new List<Repository>();
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
}