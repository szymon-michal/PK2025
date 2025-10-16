using Microsoft.AspNetCore.Mvc;
using ConnectionPlatform.Core.DTOs;
using ConnectionPlatform.Core.Interfaces;

namespace ConnectionPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        try
        {
            var user = await _userService.CreateUserAsync(createUserDto);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
    {
        var user = await _userService.UpdateUserAsync(id, updateUserDto);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost("{id}/skills")]
    public async Task<ActionResult> AddSkill(int id, [FromBody] AddSkillDto addSkillDto)
    {
        var success = await _userService.AddSkillToUserAsync(id, addSkillDto);
        if (!success)
            return BadRequest("Failed to add skill. User or skill not found, or skill already exists.");

        return Ok(new { message = "Skill added successfully" });
    }

    [HttpDelete("{id}/skills/{skillId}")]
    public async Task<ActionResult> RemoveSkill(int id, int skillId)
    {
        var success = await _userService.RemoveSkillFromUserAsync(id, skillId);
        if (!success)
            return NotFound("User skill not found");

        return Ok(new { message = "Skill removed successfully" });
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<UserDto>>> SearchUsers(
        [FromQuery] string? skill = null, 
        [FromQuery] string? interest = null, 
        [FromQuery] string? category = null)
    {
        var users = await _userService.SearchUsersAsync(skill, interest, category);
        return Ok(users);
    }
}