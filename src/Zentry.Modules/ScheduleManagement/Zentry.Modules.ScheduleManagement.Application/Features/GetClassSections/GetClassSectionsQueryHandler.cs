using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.User;
using Zentry.Modules.ScheduleManagement.Application.Services;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetClassSections;

public class GetClassSectionsQueryHandler(
    IClassSectionRepository classSectionRepository,
    IUserScheduleService userScheduleService
) : IQueryHandler<GetClassSectionsQuery, GetClassSectionsResponse>
{
    public async Task<GetClassSectionsResponse> Handle(GetClassSectionsQuery query, CancellationToken cancellationToken)
    {
        var criteria = new ClassSectionListCriteria
        {
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            SearchTerm = query.SearchTerm,
            CourseId = query.CourseId,
            LecturerId = query.LecturerId,
            StudentId = query.StudentId,
            SortBy = query.SortBy,
            SortOrder = query.SortOrder
        };

        var (classSections, totalCount) =
            await classSectionRepository.GetPagedClassSectionsAsync(criteria, cancellationToken);

        var lecturerIds = classSections
            .Select(cs => cs.LecturerId)
            .Distinct()
            .ToList();

        var lecturerLookupTasks = lecturerIds
            .Select<Guid, Task<GetUserByIdAndRoleIntegrationResponse?>>(id =>
                userScheduleService.GetUserByIdAndRoleAsync("Lecturer", id, cancellationToken))
            .ToList();

        await Task.WhenAll(lecturerLookupTasks);

        var lecturers = lecturerLookupTasks
            .Where(t => t.Result != null)
            .Select(t => t.Result!)
            .ToDictionary<GetUserByIdAndRoleIntegrationResponse, Guid, GetUserByIdAndRoleIntegrationResponse>(
                t => t.UserId,
                t => t
            );

        var classSectionDtos = classSections.Select(cs =>
        {
            lecturers.TryGetValue(cs.LecturerId, out var lecturerInfo);

            return new ClassSectionListItemDto
            {
                Id = cs.Id,
                SectionCode = cs.SectionCode,
                Semester = cs.Semester,
                CourseId = cs.CourseId,
                CourseCode = cs.Course?.Code,
                CourseName = cs.Course?.Name,
                LecturerId = cs.LecturerId,
                LecturerFullName = lecturerInfo?.FullName,
                NumberOfStudents = cs.Enrollments?.Count ?? 0
            };
        }).ToList();

        return new GetClassSectionsResponse
        {
            Items = classSectionDtos,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }
}
