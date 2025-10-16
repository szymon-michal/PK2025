namespace ConnectionPlatform.Core.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CategoryType Type { get; set; }
}

public enum CategoryType
{
    Academic,
    Professional,
    Hobby,
    Industry,
    Technology
}