using FluentValidation;
using TaskTrackerApi.DTO.Tasks;

namespace TaskTrackerApi.Validators;

public class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title could not be empty")
            .MaximumLength(255).WithMessage("Task title could not be longer than 255 characters");
        
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Task description could not be longer than 1000 characters");
    }
}