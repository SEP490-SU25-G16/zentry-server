using Zentry.Modules.ScheduleManagement.Domain.Enums;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.CreateSchedule;

public class CreateScheduleCommand : ICommand<CreatedScheduleResponse>
{
    public CreateScheduleCommand(
        Guid lecturerId,
        Guid courseId,
        Guid roomId,
        DateTime startTime,
        DateTime endTime,
        string dayOfWeekString)
    {
        LecturerId = lecturerId;
        CourseId = courseId;
        RoomId = roomId;
        StartTime = startTime;
        EndTime = endTime;
        DayOfWeek = DayOfWeekEnum.FromName(dayOfWeekString);
    }

    public CreateScheduleCommand(
        Guid lecturerId,
        Guid courseId,
        Guid roomId,
        DateTime startTime,
        DateTime endTime,
        DayOfWeekEnum dayOfWeek)
    {
        LecturerId = lecturerId;
        CourseId = courseId;
        RoomId = roomId;
        StartTime = startTime;
        EndTime = endTime;
        DayOfWeek = dayOfWeek;
    }

    public CreateScheduleCommand(
        CreateScheduleRequest request)
    {
        LecturerId = request.LecturerId;
        CourseId = request.CourseId;
        RoomId = request.RoomId;
        StartTime = request.StartTime;
        EndTime = request.EndTime;
        DayOfWeek = DayOfWeekEnum.FromName(request.DayOfWeek);
    }

    public Guid LecturerId { get; set; }
    public Guid CourseId { get; set; }
    public Guid RoomId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DayOfWeekEnum DayOfWeek { get; set; }

    public bool IsValidTimeRange()
    {
        return StartTime < EndTime;
    }
}