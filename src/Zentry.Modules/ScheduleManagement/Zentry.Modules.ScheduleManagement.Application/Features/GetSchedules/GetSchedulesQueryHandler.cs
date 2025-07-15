using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetSchedules;

public class GetSchedulesQueryHandler(
    IScheduleRepository scheduleRepository,
    ICourseRepository courseRepository,
    IRoomRepository roomRepository,
    IUserScheduleService lecturerLookupService)
    : IQueryHandler<GetSchedulesQuery, GetSchedulesResponse>
{
    public async Task<GetSchedulesResponse> Handle(GetSchedulesQuery query, CancellationToken cancellationToken)
    {
        var criteria = new ScheduleListCriteria
        {
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            LecturerId = query.LecturerId,
            CourseId = query.CourseId,
            RoomId = query.RoomId,
            DayOfWeek = query.DayOfWeek,
            SortBy = query.SortBy,
            SortOrder = query.SortOrder,
            SearchTerm = query.SearchTerm
        };

        // Sử dụng method với Include
        var (schedules, totalCount) = await scheduleRepository
            .GetPagedSchedulesWithIncludesAsync(criteria, cancellationToken);

        // Chỉ cần lookup Lecturer vì Course và Room đã được Include
        var lecturerIds = schedules.Select(s => s.LecturerId).Distinct().ToList();
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
            LecturerId = s.LecturerId,
            LecturerName = lecturerNames.GetValueOrDefault(s.LecturerId, "Unknown Lecturer"),
            CourseId = s.CourseId,
            CourseName = s.Course?.Name ?? "Unknown Course",
            RoomId = s.RoomId,
            RoomName = s.Room?.RoomName ?? "Unknown Room",
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            DayOfWeek = s.DayOfWeek,
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