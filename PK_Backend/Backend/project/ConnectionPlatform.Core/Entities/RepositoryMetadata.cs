namespace ConnectionPlatform.Core.Entities;

public class RepositoryMetadata
{
    public int RepositoryId { get; set; }
    public int TotalFiles { get; set; } = 0;
    public int TotalFolders { get; set; } = 0;
    public long TotalSize { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModified { get; set; }
    public string? License { get; set; }
    public RepositoryVisibility Visibility { get; set; } = RepositoryVisibility.Private;

    // Navigation property
    public Repository Repository { get; set; } = null!;
}

public enum RepositoryVisibility
{
    Public,
    Private
}