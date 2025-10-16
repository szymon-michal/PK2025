namespace ConnectionPlatform.Core.Entities;

public class UserFriends
{
    public int UserId { get; set; }
    public int[] Friends { get; set; } = Array.Empty<int>();

    // Navigation property
    public User User { get; set; } = null!;
}