namespace ConnectionPlatform.Core.Interfaces;

using ConnectionPlatform.Core.DTOs;

public interface IMatchmakingService
{
    Task<List<UserDto>> GetMatchSuggestionsAsync(int userId, int skip = 0, int take = 10);
    Task<double> CalculateCompatibilityScoreAsync(int userId1, int userId2);
}