using FluentValidation;
using TaskTrackerApi.DTO.Auth;

namespace TaskTrackerApi.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username could not be empty")
            .MaximumLength(255).WithMessage("Username could not be longer than 255 characters");
        
        RuleFor(x => x.Password)
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .MaximumLength(255).WithMessage("Password could not be longer than 255 characters")
            .Matches(@"[^\w\s]").WithMessage("Password must contain at least one special character");
    }
}