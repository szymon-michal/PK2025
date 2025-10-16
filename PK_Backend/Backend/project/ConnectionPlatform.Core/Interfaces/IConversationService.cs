namespace ConnectionPlatform.Core.Interfaces;

using ConnectionPlatform.Core.DTOs;

public interface IConversationService
{
    Task<ConversationDto> CreateDirectConversationAsync(int userId, int otherUserId);
    Task<List<ConversationDto>> GetUserConversationsAsync(int userId);
    Task<MessageDto> SendMessageAsync(long conversationId, int senderId, CreateMessageDto messageDto);
    Task<List<MessageDto>> GetConversationMessagesAsync(long conversationId, int userId, long cursor = 0, int limit = 20);
    Task<MessageDto?> EditMessageAsync(long messageId, int userId, string newContent);
    Task<bool> DeleteMessageAsync(long messageId, int userId);
    Task<bool> MarkMessageAsReadAsync(long messageId, int userId);
}