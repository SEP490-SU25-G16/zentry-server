using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetSchedules;

public class GetSchedulesQueryHandler(
    IScheduleRepository scheduleRepository,
    IRoomRepository roomRepository,
    IUserScheduleService lecturerLookupService,
    IClassSectionRepository classSectionRepository
) : IQueryHandler<GetSchedulesQuery, GetSchedulesResponse>
{
    public async Task<GetSchedulesResponse> Handle(GetSchedulesQuery query, CancellationToken cancellationToken)
    {
        var criteria = new ScheduleListCriteria
        {
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            LecturerId = query.LecturerId,
            ClassSectionId = query.ClassSectionId,
            RoomId = query.RoomId,
            WeekDay = query.WeekDay,
            SortBy = query.SortBy,
            SortOrder = query.SortOrder,
            SearchTerm = query.SearchTerm
        };

        var (schedules, totalCount) =
            await scheduleRepository.GetPagedSchedulesWithIncludesAsync(criteria, cancellationToken);

        var lecturerIds = schedules.Select(s => s.ClassSection?.LecturerId ?? Guid.Empty).Distinct()
            .Where(id => id != Guid.Empty).ToList();
        var lecturerNames = new Dictionary<Guid, string>();

        foreach (var lecturerId in lecturerIds)
        {
            var lecturer =
                await lecturerLookupService.GetUserByIdAndRoleAsync("Lecturer", lecturerId, cancellationToken);
            if (lecturer != null)
                lecturerNames[lecturerId] = lecturer.FullName;
        }

        var scheduleDtos = schedules.Select(s => new ScheduleDto
        {
            Id = s.Id,
            LecturerId = s.ClassSection?.LecturerId ?? Guid.Empty,
            LecturerName =
                lecturerNames.GetValueOrDefault(s.ClassSection?.LecturerId ?? Guid.Empty, "Unknown Lecturer"),
            CourseId = s.ClassSection?.CourseId ?? Guid.Empty,
            CourseName = s.ClassSection?.Course?.Name ?? "Unknown Course",
            ClassSectionId = s.ClassSectionId,
            RoomId = s.RoomId,
            RoomName = s.Room?.RoomName ?? "Unknown Room",
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            WeekDay = s.WeekDay,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        }).ToList();

        return new GetSchedulesResponse
        {
            Schedules = scheduleDtos,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }
}
