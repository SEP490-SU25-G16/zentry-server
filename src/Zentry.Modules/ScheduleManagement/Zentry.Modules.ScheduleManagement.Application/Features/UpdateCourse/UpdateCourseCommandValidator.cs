using FluentValidation;
using Zentry.Modules.ScheduleManagement.Application.Features.CreateCourse;
using Zentry.SharedKernel.Abstractions.Models;

// Assuming BaseValidator uses this

namespace Zentry.Modules.ScheduleManagement.Application.Features.UpdateCourse;
// Hoặc một namespace phù hợp khác

public class UpdateCourseCommandValidator : BaseValidator<UpdateCourseRequest>
{
    public UpdateCourseCommandValidator()
    {
        // Rule for Name
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Tên khóa học là bắt buộc.")
            .MaximumLength(200)
            .WithMessage("Tên khóa học không được vượt quá 200 ký tự.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Mô tả khóa học không được vượt quá 500 ký tự.");
    }
}
