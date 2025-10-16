using Microsoft.EntityFrameworkCore;
using ConnectionPlatform.Core.DTOs;
using ConnectionPlatform.Core.Interfaces;
using ConnectionPlatform.Infrastructure.Data;

namespace ConnectionPlatform.Infrastructure.Services;

public class MatchmakingService : IMatchmakingService
{
    private readonly ApplicationDbContext _context;
    private readonly IUserService _userService;

    public MatchmakingService(ApplicationDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public async Task<List<UserDto>> GetMatchSuggestionsAsync(int userId, int skip = 0, int take = 10)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return new List<UserDto>();

        // Get user's friends to exclude them
        var userFriends = await _context.UserFriends.FirstOrDefaultAsync(uf => uf.UserId == userId);
        var friendIds = userFriends?.Friends ?? Array.Empty<int>();

        // Get user's blocked users to exclude them
        var userBlocked = await _context.UserBlocked.FirstOrDefaultAsync(ub => ub.UserId == userId);
        var blockedIds = userBlocked?.BlockedUsers ?? Array.Empty<int>();

        // Get all other active users (excluding self, friends, and blocked)
        var allExcludedIds = friendIds.Concat(blockedIds).Append(userId).Distinct().ToList();
        var potentialMatches = await _context.Users
            .Where(u => u.IsActive && !allExcludedIds.Contains(u.Id))
            .ToListAsync();

        // Calculate compatibility scores for each potential match
        var matchScores = new List<(UserDto user, double score)>();
        
        foreach (var match in potentialMatches)
        {
            var score = await CalculateCompatibilityScoreAsync(userId, match.Id);
            var matchDto = await _userService.GetUserByIdAsync(match.Id);
            if (matchDto != null && score > 0)
            {
                matchScores.Add((matchDto, score));
            }
        }

        // Sort by compatibility score and apply pagination
        return matchScores
            .OrderByDescending(m => m.score)
            .Skip(skip)
            .Take(take)
            .Select(m => m.user)
            .ToList();
    }

    public async Task<double> CalculateCompatibilityScoreAsync(int userId1, int userId2)
    {
        // Get user skills
        var user1Skills = await _context.UserSkills
            .Where(us => us.UserId == userId1)
            .Select(us => us.SkillId)
            .ToListAsync();

        var user2Skills = await _context.UserSkills
            .Where(us => us.UserId == userId2)
            .Select(us => us.SkillId)
            .ToListAsync();

        // Get user interests
        var user1Interests = await _context.UserInterests
            .Where(ui => ui.UserId == userId1)
            .Select(ui => ui.CategoryIds)
            .FirstOrDefaultAsync() ?? Array.Empty<int>();

        var user2Interests = await _context.UserInterests
            .Where(ui => ui.UserId == userId2)
            .Select(ui => ui.CategoryIds)
            .FirstOrDefaultAsync() ?? Array.Empty<int>();

        // Calculate similarity scores
        double skillSimilarity = CalculateJaccardSimilarity(user1Skills, user2Skills);
        double interestSimilarity = CalculateJaccardSimilarity(user1Interests.ToList(), user2Interests.ToList());

        // Age compatibility (optional)
        var user1 = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId1);
        var user2 = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId2);
        
        double ageCompatibility = 1.0;
        if (user1?.Age != null && user2?.Age != null)
        {
            int ageDifference = Math.Abs(user1.Age.Value - user2.Age.Value);
            ageCompatibility = Math.Max(0, 1.0 - (ageDifference / 20.0)); // Penalize large age gaps
        }

        // Weighted average of different factors
        double finalScore = (skillSimilarity * 0.4) + (interestSimilarity * 0.4) + (ageCompatibility * 0.2);
        
        return Math.Round(finalScore, 3);
    }

    private double CalculateJaccardSimilarity(List<int> set1, List<int> set2)
    {
        if (!set1.Any() && !set2.Any()) return 1.0; // Both empty
        if (!set1.Any() || !set2.Any()) return 0.0; // One empty

        var intersection = set1.Intersect(set2).Count();
        var union = set1.Union(set2).Count();
        
        return (double)intersection / union;
    }
}