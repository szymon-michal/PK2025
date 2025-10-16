namespace ConnectionPlatform.Core.DTOs;

public class CreateFileDto
{
    public string Name { get; set; } = string.Empty;
    public int RepositoryId { get; set; }
    public int? ParentId { get; set; }
    public string Extension { get; set; } = "txt";
    public string Content { get; set; } = string.Empty;
}