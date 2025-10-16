namespace ConnectionPlatform.Core.DTOs;

public class ConversationDto
{
    public long Id { get; set; }
    public int OtherUserId { get; set; }
    public string OtherUserNick { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
}