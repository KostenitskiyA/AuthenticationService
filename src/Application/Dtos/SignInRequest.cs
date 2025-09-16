using Application.Interfaces;
using FluentValidation;

namespace Application.Dtos;

public record SignInRequest
{
    public required string Name { get; init; }

    public required string Email { get; init; }

    public required string Password { get; init; }
}

public sealed class SignInRequestValidator : AbstractValidator<SignInRequest>
{
    public SignInRequestValidator(IUserRepository userRepository)
    {
        RuleLevelCascadeMode = CascadeMode.Continue;

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(async (email, ct) =>
                !await userRepository.IsExistsAsync(user => user.Email == email.Trim().ToLowerInvariant(), ct))
            .WithMessage("User with this email already exists");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(64);
    }
}