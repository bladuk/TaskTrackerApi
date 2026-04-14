using FluentValidation;
using TaskTrackerApi.DTO.Projects;

namespace TaskTrackerApi.Validators;

public class CreateProjectDtoValidator : AbstractValidator<CreateProjectDto>
{
    public CreateProjectDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name could not be empty")
            .MaximumLength(255).WithMessage("Project name could not be longer than 255 characters");
        
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Project description could not be longer than 1000 characters");
    }
}