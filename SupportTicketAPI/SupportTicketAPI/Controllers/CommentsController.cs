using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTicketAPI.DTOs.TicketComment;
using SupportTicketAPI.Middleware;
using SupportTicketAPI.Models;
using SupportTicketAPI.Services;

namespace SupportTicketAPI.Controllers;

[ApiController]
[Route("comments")]
[Authorize]
public class CommentsController(
    ICommentService commentService,
    IValidator<CommentDto> commentValidator) : ControllerBase
{
    private int UserId => User.GetUserId();
    private RoleName Role => User.GetRole();

    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(CommentResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> EditComment(int id, [FromBody] CommentDto dto)
    {
        var validation = await commentValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        var (comment, error) = await commentService.EditCommentAsync(id, dto, UserId, Role);
        return error switch
        {
            "not_found" => NotFound(),
            "forbidden" => Forbid(),
            _ => Ok(comment)
        };
    }


    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var error = await commentService.DeleteCommentAsync(id, UserId, Role);
        return error switch
        {
            "not_found" => NotFound(),
            "forbidden" => Forbid(),
            _ => NoContent()
        };
    }
}
