namespace ConnectionPlatform.Core.Entities;

public class FriendRequest
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public string? Message { get; set; }
    public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User Sender { get; set; } = null!;
    public User Receiver { get; set; } = null!;
}

public enum FriendRequestStatus
{
    Pending,
    Accepted,
    Rejected
}