namespace ConnectionPlatform.Core.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Nick { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public int? Age { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<SkillDto> Skills { get; set; } = new();
    public List<InterestDto> Interests { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
}