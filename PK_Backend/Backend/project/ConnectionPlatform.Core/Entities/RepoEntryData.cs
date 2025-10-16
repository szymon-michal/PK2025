namespace ConnectionPlatform.Core.Entities;

public class RepoEntryData
{
    public int EntryId { get; set; }
    public bool IsDirectory { get; set; } = false;
    public FileExtension? Extension { get; set; }
    public string? Content { get; set; }
    public int? NumberOfLines { get; set; }
    public int Size { get; set; } = 0;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public RepoEntry Entry { get; set; } = null!;
}

public enum FileExtension
{
    txt,
    py,
    java,
    cpp,
    js,
    html,
    css,
    json,
    xml,
    md,
    kt
}