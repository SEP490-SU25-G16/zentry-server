namespace Zentry.SharedKernel.Contracts.Events;

public record AssignLecturerMessage(
    Guid ScheduleId,
    Guid LecturerId
);
