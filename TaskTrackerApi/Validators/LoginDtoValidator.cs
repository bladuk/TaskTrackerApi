using FluentValidation;
using TaskTrackerApi.DTO.Auth;

namespace TaskTrackerApi.Validators;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username could not be empty");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password could not be empty");
    }
}