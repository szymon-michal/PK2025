namespace ConnectionPlatform.Core.Interfaces;

/// <summary>
/// Interface for voice and video communication services.
/// This is a placeholder for future implementation.
/// </summary>
public interface ICommunicationService
{
    /// <summary>
    /// Initiates a voice call between two users.
    /// </summary>
    Task<VoiceCallResult> StartVoiceCallAsync(int callerId, int receiverId);
    
    /// <summary>
    /// Initiates a video call between two users.
    /// </summary>
    Task<VideoCallResult> StartVideoCallAsync(int callerId, int receiverId);
    
    /// <summary>
    /// Ends an active call.
    /// </summary>
    Task<bool> EndCallAsync(string callId);
    
    /// <summary>
    /// Gets the status of an active call.
    /// </summary>
    Task<CallStatus> GetCallStatusAsync(string callId);
}

public class VoiceCallResult
{
    public string CallId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
}

public class VideoCallResult
{
    public string CallId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string VideoRoomUrl { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
}

public class CallStatus
{
    public string CallId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int CallerId { get; set; }
    public int ReceiverId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public TimeSpan Duration { get; set; }
}