using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTicketAPI.DTOs;
using SupportTicketAPI.DTOs.Ticket;
using SupportTicketAPI.DTOs.TicketComment;
using SupportTicketAPI.Middleware;
using SupportTicketAPI.Models;
using SupportTicketAPI.Services;

namespace SupportTicketAPI.Controllers;

[ApiController]
[Route("tickets")]
[Authorize]
public class TicketsController(
    ITicketService ticketService,
    ICommentService commentService,
    IValidator<CreateTicketDto> createTicketValidator,
    IValidator<AssignTicketDto> assignValidator,
    IValidator<UpdateStatusDto> updateStatusValidator,
    IValidator<CommentDto> commentValidator) : ControllerBase
{
    private int UserId => User.GetUserId();
    private RoleName Role => User.GetRole();

    // ─── Tickets ─────────────────────────────────────────────────────────────

    [HttpPost]
    [Authorize(Roles = "User,Manager")]
    [ProducesResponseType(typeof(TicketResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto dto)
    {
        var validation = await createTicketValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        var ticket = await ticketService.CreateTicketAsync(dto, UserId);
        return StatusCode(201, ticket);
    }

 
    [HttpGet]
    [ProducesResponseType(typeof(List<TicketResponseDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetTickets()
    {
        var tickets = await ticketService.GetTicketsAsync(UserId, Role);
        return Ok(tickets);
    }

    /// <summary>Assign ticket (MANAGER, SUPPORT)</summary>
    [HttpPatch("{id}/assign")]
    [Authorize(Roles = "Manager,Support")]
    [ProducesResponseType(typeof(TicketResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AssignTicket(int id, [FromBody] AssignTicketDto dto)
    {
        var validation = await assignValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        var (ticket, error) = await ticketService.AssignTicketAsync(id, dto, UserId, Role);
        return error switch
        {
            "not_found" => NotFound(),
            "forbidden" => Forbid(),
            not null => BadRequest(new { message = error }),
            _ => Ok(ticket)
        };
    }

 
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Manager,Support")]
    [ProducesResponseType(typeof(TicketResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
    {
        var validation = await updateStatusValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        var (ticket, error) = await ticketService.UpdateStatusAsync(id, dto, UserId, Role);
        return error switch
        {
            "not_found" => NotFound(),
            "forbidden" => Forbid(),
            not null => BadRequest(new { message = error }),
            _ => Ok(ticket)
        };
    }


    [HttpDelete("{id}")]
    [Authorize(Roles = "Manager")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        var error = await ticketService.DeleteTicketAsync(id);
        return error switch
        {
            "not_found" => NotFound(),
            _ => NoContent()
        };
    }

    // ─── Comments ─────────────────────────────────────────────────────────────

    /// <summary>Add comment to ticket</summary>
    [HttpPost("{id}/comments")]
    [ProducesResponseType(typeof(CommentResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AddComment(int id, [FromBody] CommentDto dto)
    {
        var validation = await commentValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        var (comment, error) = await commentService.AddCommentAsync(id, dto, UserId, Role);
        return error switch
        {
            "not_found" => NotFound(),
            "forbidden" => Forbid(),
            _ => StatusCode(201, comment)
        };
    }

    /// <summary>List comments for a ticket</summary>
    [HttpGet("{id}/comments")]
    [ProducesResponseType(typeof(List<CommentResponseDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetComments(int id)
    {
        var (comments, error) = await commentService.GetCommentsAsync(id, UserId, Role);
        return error switch
        {
            "not_found" => NotFound(),
            "forbidden" => Forbid(),
            _ => Ok(comments)
        };
    }
}
