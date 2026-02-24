using Microsoft.EntityFrameworkCore;
using SupportTicketAPI.Data;
using SupportTicketAPI.DTOs;
using SupportTicketAPI.DTOs.TicketComment;
using SupportTicketAPI.Models;

namespace SupportTicketAPI.Services;

public interface ICommentService
{
    Task<(CommentResponseDto? comment, string? error)> AddCommentAsync(int ticketId, CommentDto dto, int userId, RoleName role);
    Task<(List<CommentResponseDto>? comments, string? error)> GetCommentsAsync(int ticketId, int userId, RoleName role);
    Task<(CommentResponseDto? comment, string? error)> EditCommentAsync(int commentId, CommentDto dto, int userId, RoleName role);
    Task<string?> DeleteCommentAsync(int commentId, int userId, RoleName role);
}

public class CommentService(AppDbContext db) : ICommentService
{
    private async Task<bool> CanAccessTicketAsync(Ticket ticket, int userId, RoleName role)
    {
        return role switch
        {
            RoleName.Manager => true,
            RoleName.Support => ticket.AssignedToId == userId,
            RoleName.User => ticket.CreatedById == userId,
            _ => false
        };
    }

    public async Task<(CommentResponseDto? comment, string? error)> AddCommentAsync(
        int ticketId, CommentDto dto, int userId, RoleName role)
    {
        var ticket = await db.Tickets.FindAsync(ticketId);
        if (ticket is null) return (null, "not_found");

        if (!await CanAccessTicketAsync(ticket, userId, role))
            return (null, "forbidden");

        var comment = new TicketComment
        {
            TicketId = ticketId,
            UserId = userId,
            Comment = dto.Comment
        };

        db.TicketComments.Add(comment);
        await db.SaveChangesAsync();

        await db.Entry(comment).Reference(c => c.User).LoadAsync();
        await db.Entry(comment.User).Reference(u => u.Role).LoadAsync();

        return (Mapper.ToDto(comment), null);
    }

    public async Task<(List<CommentResponseDto>? comments, string? error)> GetCommentsAsync(
        int ticketId, int userId, RoleName role)
    {
        var ticket = await db.Tickets.FindAsync(ticketId);
        if (ticket is null) return (null, "not_found");

        if (!await CanAccessTicketAsync(ticket, userId, role))
            return (null, "forbidden");

        var comments = await db.TicketComments
            .Include(c => c.User).ThenInclude(u => u.Role)
            .Where(c => c.TicketId == ticketId)
            .ToListAsync();

        return (comments.Select(Mapper.ToDto).ToList(), null);
    }

    public async Task<(CommentResponseDto? comment, string? error)> EditCommentAsync(
        int commentId, CommentDto dto, int userId, RoleName role)
    {
        var comment = await db.TicketComments
            .Include(c => c.User).ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment is null) return (null, "not_found");

        // Only author or MANAGER can edit
        if (role != RoleName.Manager && comment.UserId != userId)
            return (null, "forbidden");

        comment.Comment = dto.Comment;
        await db.SaveChangesAsync();

        return (Mapper.ToDto(comment), null);
    }

    public async Task<string?> DeleteCommentAsync(int commentId, int userId, RoleName role)
    {
        var comment = await db.TicketComments.FindAsync(commentId);
        if (comment is null) return "not_found";

        if (role != RoleName.Manager && comment.UserId != userId)
            return "forbidden";

        db.TicketComments.Remove(comment);
        await db.SaveChangesAsync();
        return null;
    }
}
