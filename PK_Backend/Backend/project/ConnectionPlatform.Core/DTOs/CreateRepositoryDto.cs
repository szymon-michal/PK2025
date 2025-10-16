namespace ConnectionPlatform.Core.DTOs;

public class CreateRepositoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Visibility { get; set; } = "Private";
}