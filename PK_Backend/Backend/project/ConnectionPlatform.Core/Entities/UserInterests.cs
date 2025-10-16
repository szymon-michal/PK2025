namespace ConnectionPlatform.Core.Entities;

public class UserInterests
{
    public int UserId { get; set; }
    public int[] CategoryIds { get; set; } = Array.Empty<int>();

    // Navigation property
    public User User { get; set; } = null!;
}