namespace Zentry.SharedKernel.Contracts.Events;

public record ScheduleCreatedMessage(
    Guid ScheduleId,
    Guid ClassSectionId,
    Guid RoomId,
    Guid? LecturerId,
    string WeekDay,
    TimeOnly ScheduledStartTime,
    TimeOnly ScheduledEndTime,
    DateOnly ScheduledStartDate,
    DateOnly ScheduledEndDate,
    Guid CourseId,
    DateTime CreatedAtUtc
);
