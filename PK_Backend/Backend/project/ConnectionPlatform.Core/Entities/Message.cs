namespace ConnectionPlatform.Core.Entities;

public class Message
{
    public long Id { get; set; }
    public long ConversationId { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public MessageType MessageType { get; set; }
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;

    // Navigation properties
    public Conversation Conversation { get; set; } = null!;
    public User Sender { get; set; } = null!;
    public User Receiver { get; set; } = null!;
}

public enum MessageType
{
    Text,
    ImageJPEG,
    ImageJPG,
    ImagePNG,
    ImageWEBP,
    Link,
    Voice,
    FileTXT,
    Folder,
    ZIP
}