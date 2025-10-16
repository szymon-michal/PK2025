using Microsoft.AspNetCore.Mvc;
using ConnectionPlatform.Core.DTOs;
using ConnectionPlatform.Core.Interfaces;

namespace ConnectionPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchesController : ControllerBase
{
    private readonly IMatchmakingService _matchmakingService;

    public MatchesController(IMatchmakingService matchmakingService)
    {
        _matchmakingService = matchmakingService;
    }

    [HttpGet("suggestions")]
    public async Task<ActionResult<List<UserDto>>> GetMatchSuggestions(
        [FromQuery] int userId,
        [FromQuery] int skip = 0, 
        [FromQuery] int take = 10)
    {
        if (userId <= 0)
            return BadRequest("Valid userId is required");

        var suggestions = await _matchmakingService.GetMatchSuggestionsAsync(userId, skip, take);
        return Ok(suggestions);
    }

    [HttpGet("compatibility")]
    public async Task<ActionResult<object>> GetCompatibilityScore(
        [FromQuery] int userId1,
        [FromQuery] int userId2)
    {
        if (userId1 <= 0 || userId2 <= 0)
            return BadRequest("Valid user IDs are required");

        var score = await _matchmakingService.CalculateCompatibilityScoreAsync(userId1, userId2);
        return Ok(new { userId1, userId2, compatibilityScore = score });
    }
}