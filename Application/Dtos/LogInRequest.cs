using Application.Interfaces;
using FluentValidation;

namespace Application.Dtos;

public record LogInRequest
{
    public required string Email { get; init; }

    public required string Password { get; init; }
}

public sealed class LogInRequestValidator : AbstractValidator<LogInRequest>
{
    public LogInRequestValidator(IUserRepository userRepository)
    {
        RuleLevelCascadeMode = CascadeMode.Continue;

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(64);
    }
}