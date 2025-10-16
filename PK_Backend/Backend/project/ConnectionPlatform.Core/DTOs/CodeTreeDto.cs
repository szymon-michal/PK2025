namespace ConnectionPlatform.Core.DTOs;

public class CodeTreeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDirectory { get; set; }
    public string? Extension { get; set; }
    public int Size { get; set; }
    public DateTime LastModified { get; set; }
    public List<CodeTreeDto> Children { get; set; } = new();
}