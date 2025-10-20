namespace ConnectionPlatform.Core.DTOs;

public class DashboardDto
{
    public int UserId { get; set; }
    public int PendingFriendRequests { get; set; }
    public int UnreadMessages { get; set; }
    public List<RepoSummaryDto> Repositories { get; set; } = new();
    public double AverageCompatibilityToFriends { get; set; } // 0..1
    public double PercentCompatibilityToFriends { get; set; } // 0..100
}
