namespace ConnectionPlatform.Core.Interfaces;

using ConnectionPlatform.Core.DTOs;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
    Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
    Task<bool> AddSkillToUserAsync(int userId, AddSkillDto addSkillDto);
    Task<bool> RemoveSkillFromUserAsync(int userId, int skillId);
    Task<List<UserDto>> SearchUsersAsync(string? skill = null, string? interest = null, string? category = null);
}