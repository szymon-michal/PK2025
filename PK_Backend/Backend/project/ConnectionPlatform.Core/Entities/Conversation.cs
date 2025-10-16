namespace ConnectionPlatform.Core.Entities;

public class Conversation
{
    public long Id { get; set; }
    public int User1Id { get; set; }
    public int User2Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User1 { get; set; } = null!;
    public User User2 { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}