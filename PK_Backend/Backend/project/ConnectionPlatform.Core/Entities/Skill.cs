namespace ConnectionPlatform.Core.Entities;

public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SkillCategory Category { get; set; }
}

public enum SkillCategory
{
    Programming,
    Design,
    DataScience,
    DevOps,
    Mobile,
    Web,
    Academic,
    Other
}