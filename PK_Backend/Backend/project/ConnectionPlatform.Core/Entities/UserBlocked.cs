namespace ConnectionPlatform.Core.Entities;

public class UserBlocked
{
    public int UserId { get; set; }
    public int[] BlockedUsers { get; set; } = Array.Empty<int>();

    // Navigation property
    public User User { get; set; } = null!;
}