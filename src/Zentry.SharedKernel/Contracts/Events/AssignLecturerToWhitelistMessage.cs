namespace Zentry.SharedKernel.Contracts.Events;

public record AssignLecturerToWhitelistMessage(
    Guid ScheduleId,
    Guid ClassSectionId,
    Guid? LecturerId = null
);
