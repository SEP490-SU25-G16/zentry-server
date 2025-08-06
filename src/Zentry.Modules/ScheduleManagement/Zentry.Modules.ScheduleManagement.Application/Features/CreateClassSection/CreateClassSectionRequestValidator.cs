using FluentValidation;
using Zentry.SharedKernel.Abstractions.Models;

namespace Zentry.Modules.ScheduleManagement.Application.Features.CreateClassSection;

public class CreateClassSectionRequestValidator : BaseValidator<CreateClassSectionRequest>
{
    public CreateClassSectionRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("Course ID là bắt buộc.");

        RuleFor(x => x.SectionCode)
            .NotEmpty().WithMessage("Section Code là bắt buộc.");

        RuleFor(x => x.Semester)
            .NotEmpty().WithMessage("Semester là bắt buộc.")
            .Matches("^(SP|SU|FA)\\d{2}$").WithMessage("Định dạng học kỳ không hợp lệ. Ví dụ: SP25, SU25."); // Thêm validation cho Semester
    }
}
