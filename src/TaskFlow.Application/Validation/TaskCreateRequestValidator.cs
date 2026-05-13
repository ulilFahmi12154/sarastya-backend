using FluentValidation;
using TaskFlow.Application.DTOs.Tasks;

namespace TaskFlow.Application.Validation;

public sealed class TaskCreateRequestValidator : AbstractValidator<TaskCreateRequest>
{
    public TaskCreateRequestValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).MaximumLength(2000).When(x => x.Content != null);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Priority).GreaterThanOrEqualTo(0);
    }
}
