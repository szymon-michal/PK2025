namespace ConnectionPlatform.Core.DTOs;

public class UpdateUserDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Nick { get; set; }
    public string? Bio { get; set; }
    public int? Age { get; set; }
}