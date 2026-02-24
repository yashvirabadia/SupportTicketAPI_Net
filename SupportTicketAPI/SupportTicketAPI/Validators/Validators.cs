using FluentValidation;
using SupportTicketAPI.DTOs;
using SupportTicketAPI.DTOs.Auth;
using SupportTicketAPI.DTOs.Ticket;
using SupportTicketAPI.DTOs.TicketComment;
using SupportTicketAPI.DTOs.User;

namespace SupportTicketAPI.Validators;

public class LoginValidator : AbstractValidator<LoginDTO>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class CreateUserValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}

public class CreateTicketValidator : AbstractValidator<CreateTicketDto>
{
    public CreateTicketValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MinimumLength(5).MaximumLength(255);
        RuleFor(x => x.Description).NotEmpty().MinimumLength(10);
        RuleFor(x => x.Priority).IsInEnum();
    }
}

public class UpdateStatusValidator : AbstractValidator<UpdateStatusDto>
{
    public UpdateStatusValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}

public class CommentValidator : AbstractValidator<CommentDto>
{
    public CommentValidator()
    {
        RuleFor(x => x.Comment).NotEmpty();
    }
}

public class AssignValidator : AbstractValidator<AssignTicketDto>
{
    public AssignValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}
