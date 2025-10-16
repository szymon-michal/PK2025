namespace ConnectionPlatform.Core.DTOs;

public class CreateMessageDto
{
    public string MessageType { get; set; } = "Text";
    public string Content { get; set; } = string.Empty;
}