using Microsoft.EntityFrameworkCore;
using SupportTicketAPI.Data;
using SupportTicketAPI.DTOs.User;
using SupportTicketAPI.Models;

namespace SupportTicketAPI.Services;

public interface IUserService
{
    Task<(UserResponseDto? user, string? error)> CreateUserAsync(CreateUserDto dto);
    Task<List<UserResponseDto>> GetAllUsersAsync();
}

public class UserService(AppDbContext db) : IUserService
{
    public async Task<(UserResponseDto? user, string? error)> CreateUserAsync(CreateUserDto dto)
    {
        var emailExists = await db.Users.AnyAsync(u => u.Email == dto.Email);
        if (emailExists)
            return (null, "Email already in use.");

        var role = await db.Roles.FindAsync(dto.RoleId);

        if (role is null)
            return (null, "Invalid role.");

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RoleId = dto.RoleId,
            Role = role
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return (Mapper.ToDto(user), null);
    }

    public async Task<List<UserResponseDto>> GetAllUsersAsync()
    {
        var users = await db.Users.Include(u => u.Role).ToListAsync();
        return users.Select(Mapper.ToDto).ToList();
    }

   
}
