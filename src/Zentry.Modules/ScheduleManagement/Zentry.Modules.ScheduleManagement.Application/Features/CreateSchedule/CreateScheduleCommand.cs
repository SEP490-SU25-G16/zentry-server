using Zentry.Modules.ScheduleManagement.Domain.Enums;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.CreateSchedule;
public class CreateScheduleCommand : ICommand<CreatedScheduleResponse>
{
    public CreateScheduleCommand(
        Guid lecturerId,
        Guid classSectionId,
        Guid roomId,
        DateTime startTime,
        DateTime endTime,
        string dayOfWeekString)
    {
        LecturerId = lecturerId;
        ClassSectionId = classSectionId;
        RoomId = roomId;
        StartTime = startTime;
        EndTime = endTime;
        DayOfWeek = DayOfWeekEnum.FromName(dayOfWeekString);
    }

    public CreateScheduleCommand(
        CreateScheduleRequest request)
    {
        LecturerId = request.LecturerId;
        ClassSectionId = request.ClassSectionId;
        RoomId = request.RoomId;
        StartTime = request.StartTime;
        EndTime = request.EndTime;
        DayOfWeek = DayOfWeekEnum.FromName(request.DayOfWeek);
    }

    public Guid LecturerId { get; set; }
    public Guid ClassSectionId { get; set; }
    public Guid RoomId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DayOfWeekEnum DayOfWeek { get; set; }

    public bool IsValidTimeRange() => StartTime < EndTime;
}

public class CreatedScheduleResponse
{
    public Guid Id { get; set; }
    public Guid LecturerId { get; set; }
    public Guid ClassSectionId { get; set; }
    public Guid RoomId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DayOfWeekEnum DayOfWeek { get; set; }
    public DateTime CreatedAt { get; set; }
}
