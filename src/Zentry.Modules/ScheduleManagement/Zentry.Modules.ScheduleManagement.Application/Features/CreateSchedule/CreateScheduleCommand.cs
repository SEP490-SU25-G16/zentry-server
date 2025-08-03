using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Schedule;

namespace Zentry.Modules.ScheduleManagement.Application.Features.CreateSchedule;

public class CreateScheduleCommand : ICommand<CreatedScheduleResponse>
{
    public CreateScheduleCommand(
        Guid lecturerId,
        Guid classSectionId,
        Guid roomId,
        DateOnly startDate,
        DateOnly endDate,
        TimeOnly startTime,
        TimeOnly endTime,
        string weekDayString)
    {
        LecturerId = lecturerId;
        ClassSectionId = classSectionId;
        RoomId = roomId;
        StartDate = startDate;
        EndDate = endDate;
        StartTime = startTime;
        EndTime = endTime;
        WeekDay = WeekDayEnum.FromName(weekDayString);
    }

    public CreateScheduleCommand(
        CreateScheduleRequest request)
    {
        LecturerId = request.LecturerId;
        ClassSectionId = request.ClassSectionId;
        RoomId = request.RoomId;

        StartDate = request.StartDate;
        EndDate = request.EndDate;
        StartTime = request.StartTime;
        EndTime = request.EndTime;
        WeekDay = WeekDayEnum.FromName(request.WeekDay);
    }

    public Guid LecturerId { get; set; }
    public Guid ClassSectionId { get; set; }
    public Guid RoomId { get; set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public TimeOnly StartTime { get; }
    public TimeOnly EndTime { get; }
    public WeekDayEnum WeekDay { get; set; }

    public bool IsValidTimeRange()
    {
        return StartTime < EndTime;
    }
}

public class CreatedScheduleResponse
{
    public Guid Id { get; set; }
    public Guid LecturerId { get; set; }
    public Guid ClassSectionId { get; set; }
    public Guid RoomId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? WeekDay { get; set; }
    public DateTime CreatedAt { get; set; }
}
