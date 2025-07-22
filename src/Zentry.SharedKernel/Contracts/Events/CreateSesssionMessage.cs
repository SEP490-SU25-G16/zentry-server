namespace Zentry.SharedKernel.Contracts.Events;

public record CreateSesssionMessage(
    Guid ScheduleId,
    Guid LecturerId,
    Guid ClassSectionId,
    Guid RoomId,
    string WeekDay,
    TimeOnly ScheduledStartTime,
    TimeOnly ScheduledEndTime,
    DateOnly ScheduledStartDate,
    DateOnly ScheduledEndDate,
    Guid CourseId,
    DateTime CreatedAt
);
