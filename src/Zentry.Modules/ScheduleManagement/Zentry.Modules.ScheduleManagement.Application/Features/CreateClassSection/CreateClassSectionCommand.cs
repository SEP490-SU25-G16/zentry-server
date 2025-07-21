using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.CreateClassSection;

public record CreateClassSectionCommand(
    Guid CourseId,
    Guid LecturerId,
    string SectionCode,
    string Semester
) : ICommand<CreateClassSectionResponse>;

public record CreateClassSectionResponse(Guid Id);