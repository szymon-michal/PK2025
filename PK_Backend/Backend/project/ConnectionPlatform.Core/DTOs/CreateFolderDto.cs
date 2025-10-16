namespace ConnectionPlatform.Core.DTOs;

public class CreateFolderDto
{
    public string Name { get; set; } = string.Empty;
    public int RepositoryId { get; set; }
    public int? ParentId { get; set; }
}