// File: Zentry.Modules.ScheduleManagement.Application.Features.CreateClassSection/CreateClassSectionCommandHandler.cs

using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.Modules.ScheduleManagement.Domain.ValueObjects; // Thêm Value Object Semester
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ScheduleManagement.Application.Features.CreateClassSection;

public class CreateClassSectionCommandHandler(IClassSectionRepository repository)
    : ICommandHandler<CreateClassSectionCommand, CreateClassSectionResponse>
{
    public async Task<CreateClassSectionResponse> Handle(CreateClassSectionCommand request,
        CancellationToken cancellationToken)
    {
        var semester = Semester.Create(request.Semester);
        
        var existingSection = await repository.GetBySectionCodeAndSemesterAsync(request.SectionCode, semester, cancellationToken);
        
        if (existingSection is not null)
        {
            throw new BusinessRuleException("SECTION_CODE_DUPLICATE", $"Section Code '{request.SectionCode}' đã tồn tại trong học kỳ '{request.Semester}'.");
        }

        var newSection = ClassSection.Create(
            request.CourseId,
            request.SectionCode,
            semester
        );

        await repository.AddAsync(newSection, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return new CreateClassSectionResponse(newSection.Id);
    }
}
