using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.ClassSections.GetStudentCountBySemester;

public record GetStudentCountBySemesterQuery(int Year) : IQuery<GetStudentCountBySemesterResponse>;

public record GetStudentCountBySemesterResponse(Dictionary<string, int> Semesters);
