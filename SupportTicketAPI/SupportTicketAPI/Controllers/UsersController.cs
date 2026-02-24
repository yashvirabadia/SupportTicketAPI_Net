using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTicketAPI.DTOs;
using SupportTicketAPI.DTOs.User;
using SupportTicketAPI.Services;

namespace SupportTicketAPI.Controllers;

[ApiController]
[Route("users")]
[Authorize(Roles = "Manager")]
public class UsersController(IUserService userService, IValidator<CreateUserDto> validator) : ControllerBase
{
    /// <summary>Create user (MANAGER only)</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        var (user, error) = await userService.CreateUserAsync(dto);
        if (error is not null)
            return BadRequest(new { message = error });

        return CreatedAtAction(nameof(GetUsers), null, user);
    }

    /// <summary>List users (MANAGER only)</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<UserResponseDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetUsers()
    {
        var users = await userService.GetAllUsersAsync();
        return Ok(users);
    }
}
