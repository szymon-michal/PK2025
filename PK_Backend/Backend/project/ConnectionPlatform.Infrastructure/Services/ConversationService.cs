using Microsoft.EntityFrameworkCore;
using ConnectionPlatform.Core.DTOs;
using ConnectionPlatform.Core.Entities;
using ConnectionPlatform.Core.Interfaces;
using ConnectionPlatform.Infrastructure.Data;
using System.Text;

namespace ConnectionPlatform.Infrastructure.Services;

public class ConversationService : IConversationService
{
    private readonly ApplicationDbContext _context;

    public ConversationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ConversationDto> CreateDirectConversationAsync(int userId, int otherUserId)
    {
        // Ensure user1Id < user2Id for consistency
        var user1Id = Math.Min(userId, otherUserId);
        var user2Id = Math.Max(userId, otherUserId);

        // Check if conversation already exists
        var existingConversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.User1Id == user1Id && c.User2Id == user2Id);

        if (existingConversation != null)
        {
            return await MapToConversationDto(existingConversation, userId);
        }

        // Check if users are friends
        var userFriends = await _context.UserFriends.FirstOrDefaultAsync(uf => uf.UserId == userId);
        var isFriend = userFriends?.Friends?.Contains(otherUserId) ?? false;
        
        if (!isFriend)
        {
            throw new InvalidOperationException("Users are not friends");
        }

        // Check if either user is blocked
        var userBlocked = await _context.UserBlocked.FirstOrDefaultAsync(ub => ub.UserId == userId);
        var otherUserBlocked = await _context.UserBlocked.FirstOrDefaultAsync(ub => ub.UserId == otherUserId);
        
        var isBlocked = (userBlocked?.BlockedUsers?.Contains(otherUserId) ?? false) ||
                       (otherUserBlocked?.BlockedUsers?.Contains(userId) ?? false);
        
        if (isBlocked)
        {
            throw new InvalidOperationException("Cannot create conversation with blocked user");
        }

        // Create new conversation
        var conversation = new Conversation
        {
            User1Id = user1Id,
            User2Id = user2Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        return await MapToConversationDto(conversation, userId);
    }

    public async Task<List<ConversationDto>> GetUserConversationsAsync(int userId)
    {
        var conversations = await _context.Conversations
            .Where(c => c.User1Id == userId || c.User2Id == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var result = new List<ConversationDto>();
        foreach (var conversation in conversations)
        {
            result.Add(await MapToConversationDto(conversation, userId));
        }

        return result;
    }

    public async Task<MessageDto> SendMessageAsync(long conversationId, int senderId, CreateMessageDto messageDto)
    {
        var conversation = await _context.Conversations.FirstOrDefaultAsync(c => c.Id == conversationId);
        if (conversation == null)
            throw new ArgumentException("Conversation not found");

        // Verify sender is part of conversation
        if (conversation.User1Id != senderId && conversation.User2Id != senderId)
            throw new UnauthorizedAccessException("User is not part of this conversation");

        var receiverId = conversation.User1Id == senderId ? conversation.User2Id : conversation.User1Id;

        // Convert message type
        if (!Enum.TryParse<MessageType>(messageDto.MessageType, true, out var messageType))
            messageType = MessageType.Text;

        var message = new Message
        {
            ConversationId = conversationId,
            SenderId = senderId,
            ReceiverId = receiverId,
            MessageType = messageType,
            Content = Encoding.UTF8.GetBytes(messageDto.Content),
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return await MapToMessageDto(message);
    }

    public async Task<List<MessageDto>> GetConversationMessagesAsync(long conversationId, int userId, long cursor = 0, int limit = 20)
    {
        var conversation = await _context.Conversations.FirstOrDefaultAsync(c => c.Id == conversationId);
        if (conversation == null)
            throw new ArgumentException("Conversation not found");

        // Verify user is part of conversation
        if (conversation.User1Id != userId && conversation.User2Id != userId)
            throw new UnauthorizedAccessException("User is not part of this conversation");

        var query = _context.Messages
            .Where(m => m.ConversationId == conversationId);

        if (cursor > 0)
        {
            query = query.Where(m => m.Id < cursor);
        }

        var messages = await query
            .OrderByDescending(m => m.Id)
            .Take(limit)
            .ToListAsync();

        var result = new List<MessageDto>();
        foreach (var message in messages.OrderBy(m => m.Id))
        {
            result.Add(await MapToMessageDto(message));
        }

        return result;
    }

    public async Task<MessageDto?> EditMessageAsync(long messageId, int userId, string newContent)
    {
        var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId);
        if (message == null) return null;

        // Only sender can edit their message
        if (message.SenderId != userId)
            throw new UnauthorizedAccessException("Only sender can edit message");

        // Only allow editing text messages
        if (message.MessageType != MessageType.Text)
            throw new InvalidOperationException("Only text messages can be edited");

        message.Content = Encoding.UTF8.GetBytes(newContent);
        await _context.SaveChangesAsync();

        return await MapToMessageDto(message);
    }

    public async Task<bool> DeleteMessageAsync(long messageId, int userId)
    {
        var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId);
        if (message == null) return false;

        // Only sender can delete their message
        if (message.SenderId != userId)
            throw new UnauthorizedAccessException("Only sender can delete message");

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkMessageAsReadAsync(long messageId, int userId)
    {
        var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId);
        if (message == null) return false;

        // Only receiver can mark message as read
        if (message.ReceiverId != userId)
            return false;

        message.IsRead = true;
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<ConversationDto> MapToConversationDto(Conversation conversation, int currentUserId)
    {
        var otherUserId = conversation.User1Id == currentUserId ? conversation.User2Id : conversation.User1Id;
        var otherUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == otherUserId);

        var lastMessage = await _context.Messages
            .Where(m => m.ConversationId == conversation.Id)
            .OrderByDescending(m => m.SentAt)
            .FirstOrDefaultAsync();

        var unreadCount = await _context.Messages
            .CountAsync(m => m.ConversationId == conversation.Id && 
                           m.ReceiverId == currentUserId && 
                           !m.IsRead);

        return new ConversationDto
        {
            Id = conversation.Id,
            OtherUserId = otherUserId,
            OtherUserNick = otherUser?.Nick ?? "Unknown",
            CreatedAt = conversation.CreatedAt,
            LastMessageAt = lastMessage?.SentAt,
            UnreadCount = unreadCount
        };
    }

    private async Task<MessageDto> MapToMessageDto(Message message)
    {
        var sender = await _context.Users.FirstOrDefaultAsync(u => u.Id == message.SenderId);
        var receiver = await _context.Users.FirstOrDefaultAsync(u => u.Id == message.ReceiverId);

        var content = message.MessageType == MessageType.Text 
            ? Encoding.UTF8.GetString(message.Content) 
            : Convert.ToBase64String(message.Content);

        return new MessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderId = message.SenderId,
            SenderNick = sender?.Nick ?? "Unknown",
            ReceiverId = message.ReceiverId,
            ReceiverNick = receiver?.Nick ?? "Unknown",
            MessageType = message.MessageType.ToString(),
            Content = content,
            SentAt = message.SentAt,
            IsRead = message.IsRead
        };
    }
}