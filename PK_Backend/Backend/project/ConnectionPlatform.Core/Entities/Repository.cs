namespace ConnectionPlatform.Core.Entities;

public class Repository
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public RepositoryMetadata? Metadata { get; set; }
    public ICollection<RepoEntry> Entries { get; set; } = new List<RepoEntry>();
}