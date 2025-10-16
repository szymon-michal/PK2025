using Microsoft.AspNetCore.SignalR;

namespace ConnectionPlatform.API.Hubs;

public class ChatHub : Hub
{
    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
    }

    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
    }

    public async Task SendMessage(string conversationId, string senderId, string message, string messageType = "text")
    {
        await Clients.Group($"conversation_{conversationId}").SendAsync("ReceiveMessage", new
        {
            SenderId = senderId,
            Message = message,
            MessageType = messageType,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task SendTyping(string conversationId, string senderId, bool isTyping)
    {
        await Clients.Group($"conversation_{conversationId}").SendAsync("UserTyping", new
        {
            SenderId = senderId,
            IsTyping = isTyping
        });
    }

    public async Task SendReadReceipt(string conversationId, string messageId, string userId)
    {
        await Clients.Group($"conversation_{conversationId}").SendAsync("MessageRead", new
        {
            MessageId = messageId,
            UserId = userId,
            ReadAt = DateTime.UtcNow
        });
    }

    // Placeholder methods for future voice/video functionality
    public async Task StartVoiceCall(string conversationId, string callerId)
    {
        // TODO: Implement voice call logic
        await Clients.Group($"conversation_{conversationId}").SendAsync("VoiceCallStarted", new
        {
            CallerId = callerId,
            CallId = Guid.NewGuid().ToString(),
            StartedAt = DateTime.UtcNow
        });
    }

    public async Task StartVideoCall(string conversationId, string callerId)
    {
        // TODO: Implement video call logic
        await Clients.Group($"conversation_{conversationId}").SendAsync("VideoCallStarted", new
        {
            CallerId = callerId,
            CallId = Guid.NewGuid().ToString(),
            VideoRoomUrl = "https://placeholder-video-room.com",
            StartedAt = DateTime.UtcNow
        });
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}