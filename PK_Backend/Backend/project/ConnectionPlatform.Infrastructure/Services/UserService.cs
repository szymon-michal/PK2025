using Microsoft.EntityFrameworkCore;
using ConnectionPlatform.Core.DTOs;
using ConnectionPlatform.Core.Entities;
using ConnectionPlatform.Core.Interfaces;
using ConnectionPlatform.Infrastructure.Data;
using BCrypt.Net;


namespace ConnectionPlatform.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _context.Users
            .Include(u => u.Interests)
            .Include(u => u.ProfilePhoto)
            .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);

        if (user == null) return null;

        var userSkills = await _context.UserSkills
            .Include(us => us.Skill)
            .Where(us => us.UserId == id)
            .ToListAsync();

        var interestIds = user.Interests?.CategoryIds ?? Array.Empty<int>();
        var interests = await _context.Interests
            .Where(i => interestIds.Contains(i.Id))
            .ToListAsync();

        var categories = await _context.Categories.ToListAsync(); // For demo, get all categories

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Nick = user.Nick,
            Bio = user.Bio,
            Age = user.Age,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            Skills = userSkills.Select(us => new SkillDto
            {
                Id = us.Skill.Id,
                Name = us.Skill.Name,
                Description = us.Skill.Description,
                Category = us.Skill.Category.ToString(),
                Level = us.Level.ToString()
            }).ToList(),
            Interests = interests.Select(i => new InterestDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description
            }).ToList(),
            Categories = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Type = c.Type.ToString()
            }).ToList()
        };
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        var user = new User
        {
            Email = createUserDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password), // Simple password hashing
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            Nick = createUserDto.Nick,
            Bio = createUserDto.Bio,
            Age = createUserDto.Age,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Initialize empty collections
        _context.UserInterests.Add(new UserInterests { UserId = user.Id, CategoryIds = Array.Empty<int>() });
        _context.UserFriends.Add(new UserFriends { UserId = user.Id, Friends = Array.Empty<int>() });
        _context.UserBlocked.Add(new UserBlocked { UserId = user.Id, BlockedUsers = Array.Empty<int>() });
        
        await _context.SaveChangesAsync();

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Nick = user.Nick,
            Bio = user.Bio,
            Age = user.Age,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return null;

        if (!string.IsNullOrEmpty(updateUserDto.FirstName))
            user.FirstName = updateUserDto.FirstName;
        if (!string.IsNullOrEmpty(updateUserDto.LastName))
            user.LastName = updateUserDto.LastName;
        if (!string.IsNullOrEmpty(updateUserDto.Nick))
            user.Nick = updateUserDto.Nick;
        if (updateUserDto.Bio != null)
            user.Bio = updateUserDto.Bio;
        if (updateUserDto.Age.HasValue)
            user.Age = updateUserDto.Age.Value;

        await _context.SaveChangesAsync();
        return await GetUserByIdAsync(id);
    }

    public async Task<bool> AddSkillToUserAsync(int userId, AddSkillDto addSkillDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        var skill = await _context.Skills.FirstOrDefaultAsync(s => s.Id == addSkillDto.SkillId);
        
        if (user == null || skill == null) return false;

        var existingUserSkill = await _context.UserSkills
            .FirstOrDefaultAsync(us => us.UserId == userId && us.SkillId == addSkillDto.SkillId);
        
        if (existingUserSkill != null) return false; // Skill already exists

        if (!Enum.TryParse<SkillLevel>(addSkillDto.Level, out var level))
            level = SkillLevel.Beginner;

        var userSkill = new UserSkill
        {
            UserId = userId,
            SkillId = addSkillDto.SkillId,
            Level = level,
            AddedAt = DateTime.UtcNow
        };

        _context.UserSkills.Add(userSkill);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveSkillFromUserAsync(int userId, int skillId)
    {
        var userSkill = await _context.UserSkills
            .FirstOrDefaultAsync(us => us.UserId == userId && us.SkillId == skillId);
        
        if (userSkill == null) return false;

        _context.UserSkills.Remove(userSkill);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<UserDto>> SearchUsersAsync(string? skill = null, string? interest = null, string? category = null)
    {
        var query = _context.Users.Where(u => u.IsActive);

        if (!string.IsNullOrEmpty(skill))
        {
            var skillIds = await _context.Skills
                .Where(s => s.Name.ToLower().Contains(skill.ToLower()))
                .Select(s => s.Id)
                .ToListAsync();

            var userIdsWithSkill = await _context.UserSkills
                .Where(us => skillIds.Contains(us.SkillId))
                .Select(us => us.UserId)
                .ToListAsync();

            query = query.Where(u => userIdsWithSkill.Contains(u.Id));
        }

        if (!string.IsNullOrEmpty(interest))
        {
            var interestIds = await _context.Interests
                .Where(i => i.Name.ToLower().Contains(interest.ToLower()))
                .Select(i => i.Id)
                .ToListAsync();

            var userIdsWithInterest = await _context.UserInterests
                .Where(ui => interestIds.Any(iid => ui.CategoryIds.Contains(iid)))
                .Select(ui => ui.UserId)
                .ToListAsync();

            query = query.Where(u => userIdsWithInterest.Contains(u.Id));
        }

        if (!string.IsNullOrEmpty(category))
        {
            var categoryIds = await _context.Categories
                .Where(c => c.Name.ToLower().Contains(category.ToLower()))
                .Select(c => c.Id)
                .ToListAsync();

            // For simplicity, using same logic as interests
            var userIdsWithCategory = await _context.UserInterests
                .Where(ui => categoryIds.Any(cid => ui.CategoryIds.Contains(cid)))
                .Select(ui => ui.UserId)
                .ToListAsync();

            query = query.Where(u => userIdsWithCategory.Contains(u.Id));
        }

        var users = await query.Take(50).ToListAsync(); // Limit results

        var result = new List<UserDto>();
        foreach (var user in users)
        {
            var userDto = await GetUserByIdAsync(user.Id);
            if (userDto != null)
                result.Add(userDto);
        }

        return result;
    }
}