using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Features.GetClassSections;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ScheduleManagement.Application.Features.CreateClassSection;

public class CreateClassSectionCommandHandler(IClassSectionRepository repository)
    : ICommandHandler<CreateClassSectionCommand, CreateClassSectionResponse>
{
    public async Task<CreateClassSectionResponse> Handle(CreateClassSectionCommand request,
        CancellationToken cancellationToken)
    {
        // Check uniqueness of SectionCode in the same semester
        var existingSections =
            await repository.GetBySectionCodeAsync(request.SectionCode, request.Semester, cancellationToken);

        if (existingSections is not null)
        {
            throw new BusinessRuleException("SECTION_CODE_DUPLICATE", "Section code đã tồn tại trong học kỳ này.");
        }

        var newSection = ClassSection.Create(
            request.CourseId,
            request.LecturerId,
            request.SectionCode,
            request.Semester
        );

        await repository.AddAsync(newSection, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return new CreateClassSectionResponse(newSection.Id);
    }
}
