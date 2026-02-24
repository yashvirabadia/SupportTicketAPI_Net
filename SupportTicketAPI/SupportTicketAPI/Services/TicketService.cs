using Microsoft.EntityFrameworkCore;
using SupportTicketAPI.Data;
using SupportTicketAPI.DTOs.Ticket;
using SupportTicketAPI.DTOs.User;
using SupportTicketAPI.Models;

namespace SupportTicketAPI.Services;

public interface ITicketService
{
    Task<TicketResponseDto> CreateTicketAsync(CreateTicketDto dto, int userId);
    Task<List<TicketResponseDto>> GetTicketsAsync(int userId, RoleName role);
    Task<(TicketResponseDto? ticket, string? error)> AssignTicketAsync(
        int ticketId, AssignTicketDto dto, int requesterId, RoleName requesterRole);
    Task<(TicketResponseDto? ticket, string? error)> UpdateStatusAsync(
        int ticketId, UpdateStatusDto dto, int requesterId, RoleName requesterRole);
    Task<string?> DeleteTicketAsync(int ticketId);
}

public class TicketService(AppDbContext db) : ITicketService
{
    // Valid lifecycle: Open → InProgress → Resolved → Closed
    private static readonly Dictionary<TicketStatus, TicketStatus> ValidTransitions = new()
    {
        [TicketStatus.Open] = TicketStatus.In_Progress,
        [TicketStatus.In_Progress] = TicketStatus.Resolved,
        [TicketStatus.Resolved] = TicketStatus.Closed
    };

    // ================= CREATE =================
    public async Task<TicketResponseDto> CreateTicketAsync(CreateTicketDto dto, int userId)
    {
        var ticket = new Ticket
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            CreatedById = userId,
            Status = TicketStatus.Open
        };

        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();

        var createdTicket = await db.Tickets
            .Include(t => t.CreatedBy).ThenInclude(u => u.Role)
            .Include(t => t.AssignedTo).ThenInclude(u => u.Role)
            .FirstAsync(t => t.Id == ticket.Id);

        return ToDto(createdTicket);
    }

    // ================= GET =================
    public async Task<List<TicketResponseDto>> GetTicketsAsync(int userId, RoleName role)
    {
        IQueryable<Ticket> query = db.Tickets
            .Include(t => t.CreatedBy).ThenInclude(u => u.Role)
            .Include(t => t.AssignedTo).ThenInclude(u => u.Role);

        query = role switch
        {
            RoleName.Manager => query,
            RoleName.Support => query.Where(t => t.AssignedToId == userId),
            RoleName.User => query.Where(t => t.CreatedById == userId),
            _ => query.Where(_ => false)
        };

        var tickets = await query.ToListAsync();

        return tickets.Select(ToDto).ToList();
    }

    // ================= ASSIGN =================
    public async Task<(TicketResponseDto? ticket, string? error)> AssignTicketAsync(
        int ticketId, AssignTicketDto dto, int requesterId, RoleName requesterRole)
    {
        var ticket = await db.Tickets
            .Include(t => t.CreatedBy).ThenInclude(u => u.Role)
            .Include(t => t.AssignedTo).ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket is null)
            return (null, "not_found");

        // Support can only manage tickets assigned to them (or unassigned)
        if (requesterRole == RoleName.Support &&
            ticket.AssignedToId != requesterId &&
            ticket.AssignedToId != null)
            return (null, "forbidden");

        var assignee = await db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == dto.UserId);

        if (assignee is null)
            return (null, "Assignee not found");

        if (assignee.Role.Name == RoleName.User)
            return (null, "Cannot assign ticket to User role");

        ticket.AssignedToId = assignee.Id;
        ticket.AssignedTo = assignee;

        await db.SaveChangesAsync();

        return (ToDto(ticket), null);
    }

    // ================= STATUS UPDATE =================
    public async Task<(TicketResponseDto? ticket, string? error)> UpdateStatusAsync(
        int ticketId, UpdateStatusDto dto, int requesterId, RoleName requesterRole)
    {
        var ticket = await db.Tickets
            .Include(t => t.CreatedBy).ThenInclude(u => u.Role)
            .Include(t => t.AssignedTo).ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket is null)
            return (null, "not_found");

        if (requesterRole == RoleName.Support &&
            ticket.AssignedToId != requesterId)
            return (null, "forbidden");

        if (!ValidTransitions.TryGetValue(ticket.Status, out var allowedNext) ||
            allowedNext != dto.Status)
            return (null,
                $"Invalid transition from {ticket.Status} to {dto.Status}. " +
                $"Allowed: {ticket.Status} → {allowedNext}");

        var log = new TicketStatusLog
        {
            TicketId = ticket.Id,
            OldStatus = ticket.Status,
            NewStatus = dto.Status,
            ChangedById = requesterId
        };

        ticket.Status = dto.Status;

        db.TicketStatusLogs.Add(log);
        await db.SaveChangesAsync();

        return (ToDto(ticket), null);
    }

    // ================= DELETE =================
    public async Task<string?> DeleteTicketAsync(int ticketId)
    {
        var ticket = await db.Tickets.FindAsync(ticketId);

        if (ticket is null)
            return "not_found";

        db.Tickets.Remove(ticket);
        await db.SaveChangesAsync();

        return null;
    }

    // ================= PRIVATE DTO MAPPER =================
    private static TicketResponseDto ToDto(Ticket ticket)
    {
        return new TicketResponseDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status.ToString(),
            Priority = ticket.Priority.ToString(),
            CreatedAt = ticket.CreatedAt,

            CreatedBy = new UserResponseDto
            {
                Id = ticket.CreatedBy.Id,
                Name = ticket.CreatedBy.Name,
                Email = ticket.CreatedBy.Email,
                CreatedAt = ticket.CreatedBy.CreatedAt,
                Role = new RoleResponseDto
                {
                    Id = ticket.CreatedBy.Role.Id,
                    Name = ticket.CreatedBy.Role.Name.ToString()
                }
            },

            AssignedTo = ticket.AssignedTo == null ? null : new UserResponseDto
            {
                Id = ticket.AssignedTo.Id,
                Name = ticket.AssignedTo.Name,
                Email = ticket.AssignedTo.Email,
                CreatedAt = ticket.AssignedTo.CreatedAt,
                Role = new RoleResponseDto
                {
                    Id = ticket.AssignedTo.Role.Id,
                    Name = ticket.AssignedTo.Role.Name.ToString()
                }
            }
        };
    }
}