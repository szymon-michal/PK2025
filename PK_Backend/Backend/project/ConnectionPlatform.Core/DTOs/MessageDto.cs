namespace ConnectionPlatform.Core.DTOs;

public class MessageDto
{
    public long Id { get; set; }
    public long ConversationId { get; set; }
    public int SenderId { get; set; }
    public string SenderNick { get; set; } = string.Empty;
    public int ReceiverId { get; set; }
    public string ReceiverNick { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
}