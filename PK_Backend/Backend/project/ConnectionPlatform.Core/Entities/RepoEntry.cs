namespace ConnectionPlatform.Core.Entities;

public class RepoEntry
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int RepositoryId { get; set; }
    public int? ParentId { get; set; }

    // Navigation properties
    public Repository Repository { get; set; } = null!;
    public RepoEntry? Parent { get; set; }
    public ICollection<RepoEntry> Children { get; set; } = new List<RepoEntry>();
    public RepoEntryData? Data { get; set; }
}