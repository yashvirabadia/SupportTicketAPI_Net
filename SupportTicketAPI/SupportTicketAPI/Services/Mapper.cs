using SupportTicketAPI.DTOs;
using SupportTicketAPI.DTOs.Ticket;
using SupportTicketAPI.DTOs.TicketComment;
using SupportTicketAPI.DTOs.User;
using SupportTicketAPI.Models;

namespace SupportTicketAPI.Services;

public static class Mapper
{
    public static RoleResponseDto ToDto(Role role) => new()
    {
        Id = role.Id,
        Name = role.Name.ToString()
    };

    public static UserResponseDto ToDto(User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        Role = ToDto(user.Role),
        CreatedAt = user.CreatedAt
    };

    public static TicketResponseDto ToDto(Ticket ticket) => new()
    {
        Id = ticket.Id,
        Title = ticket.Title,
        Description = ticket.Description,
        Status = ticket.Status.ToString(),
        Priority = ticket.Priority.ToString(),
        CreatedBy = ToDto(ticket.CreatedBy),
        AssignedTo = ticket.AssignedTo is null ? null : ToDto(ticket.AssignedTo),
        CreatedAt = ticket.CreatedAt
    };

    public static CommentResponseDto ToDto(TicketComment comment) => new()
    {
        Id = comment.Id,
        Comment = comment.Comment,
        User = ToDto(comment.User),
        CreatedAt = comment.CreatedAt
    };
}
