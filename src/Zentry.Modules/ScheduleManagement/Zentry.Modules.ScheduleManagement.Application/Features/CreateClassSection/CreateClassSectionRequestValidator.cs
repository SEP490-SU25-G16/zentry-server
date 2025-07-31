using FluentValidation;
using Zentry.SharedKernel.Abstractions.Models;

namespace Zentry.Modules.ScheduleManagement.Application.Features.CreateClassSection;

public class CreateClassSectionRequestValidator : BaseValidator<CreateClassSectionRequest>
{
    public CreateClassSectionRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("Course ID là bắt buộc.");

        RuleFor(x => x.LecturerId)
            .NotEmpty().WithMessage("Lecturer ID là bắt buộc.");

        RuleFor(x => x.SectionCode)
            .NotEmpty().WithMessage("Section Code là bắt buộc.");

        RuleFor(x => x.Semester)
            .NotEmpty().WithMessage("Semester là bắt buộc.");
    }
}