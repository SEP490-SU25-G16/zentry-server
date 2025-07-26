using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.Modules.ScheduleManagement.Application.Services;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.User;
using Zentry.SharedKernel.Enums.User;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetSchedules;

public class GetSchedulesQueryHandler(
    IScheduleRepository scheduleRepository,
    IUserScheduleService lecturerLookupService
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

        var lecturerIds = schedules
            .Where(s => s.ClassSection?.LecturerId != null) // Lọc các Schedule có ClassSection và LecturerId
            .Select(s => s.ClassSection!.LecturerId)
            .Distinct()
            .ToList();

        var lecturerLookupTasks = lecturerIds
            .Select(id => lecturerLookupService.GetUserByIdAndRoleAsync(Role.Lecturer, id, cancellationToken))
            .ToList();

        await Task.WhenAll(lecturerLookupTasks);

        // Xử lý kết quả lookup để tạo dictionary
        // Specify type arguments explicitly for ToDictionary
        var lecturers = lecturerLookupTasks
            .Where(t => t.Result != null)
            .Select(t => t.Result!) // Khẳng định Result không null
            .ToDictionary<GetUserByIdAndRoleIntegrationResponse, Guid, GetUserByIdAndRoleIntegrationResponse>(
                t => t.UserId,
                t => t
            );

        var scheduleDtos = schedules.Select(s =>
        {
            lecturers.TryGetValue(s.ClassSection?.LecturerId ?? Guid.Empty, out var lecturerInfo);

            return new ScheduleDto
            {
                Id = s.Id,
                ClassSectionId = s.ClassSectionId,
                ClassSectionCode = s.ClassSection?.SectionCode,
                LecturerId = s.ClassSection?.LecturerId ?? Guid.Empty,
                LecturerName = lecturerInfo?.FullName ?? "Unknown Lecturer",
                CourseId = s.ClassSection?.CourseId ?? Guid.Empty,
                CourseCode = s.ClassSection?.Course?.Code,
                CourseName = s.ClassSection?.Course?.Name ?? "Unknown Course",
                RoomId = s.RoomId,
                RoomName = s.Room?.RoomName ?? "Unknown Room",
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                WeekDay = s.WeekDay.ToString(),
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            };
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