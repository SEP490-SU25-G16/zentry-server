using MediatR;
using Microsoft.AspNetCore.Http;
using Zentry.Modules.Schedule.Application.Abstractions;
using Zentry.Modules.Schedule.Application.Dtos;

namespace Zentry.Modules.Schedule.Application.Features.ViewStudentSchedule;

public class ViewStudentScheduleHandler(
    IScheduleRepository scheduleRepository,
    // IEnrollmentService enrollmentService,
    // IUserService userService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<ViewStudentScheduleRequest, List<ScheduleDto>>
{
    // private readonly IEnrollmentService _enrollmentService;
    // private readonly IUserService _userService;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IScheduleRepository _scheduleRepository = scheduleRepository;

    // _enrollmentService = enrollmentService;
    // _userService = userService;

    public Task<List<ScheduleDto>> Handle(ViewStudentScheduleRequest request, CancellationToken cancellationToken)
    {
        // var studentId = Guid.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        //                            ?? throw new UnauthorizedAccessException("Student ID not found in token."));
        //
        // var enrollments = await _enrollmentService.GetEnrollmentsByStudentIdAsync(studentId);
        // var courseIds = enrollments.Select(e => e.CourseId).ToList();
        //
        // var schedules = await _scheduleRepository.GetSchedulesByCourseIdsAsync(courseIds, request.StartDate, request.EndDate);
        //
        // var lecturerIds = schedules.Select(s => s.Course.LecturerId).Distinct().ToList();
        // var lecturers = await _userService.GetUsersByIdsAsync(lecturerIds);
        //
        // return schedules.Select(s => new ScheduleDto
        // {
        //     ScheduleId = s.Id,
        //     CourseName = s.Course.Name,
        //     LecturerName = lecturers.FirstOrDefault(u => u.Id == s.Course.LecturerId)?.FullName ?? "Unknown",
        //     RoomName = s.Room.Name,
        //     StartTime = s.StartTime,
        //     EndTime = s.EndTime
        // }).ToList();
        throw new NotImplementedException();
    }
}