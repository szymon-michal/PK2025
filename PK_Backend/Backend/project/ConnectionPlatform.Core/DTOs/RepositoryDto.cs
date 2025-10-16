namespace ConnectionPlatform.Core.DTOs;

public class RepositoryDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalFiles { get; set; }
    public int TotalFolders { get; set; }
    public long TotalSize { get; set; }
    public string Visibility { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModified { get; set; }
}