namespace Zentry.SharedKernel.Contracts.Events;

public record GenerateScheduleWhitelistMessage(
    Guid ScheduleId,
    Guid ClassSectionId,
    Guid? LecturerId = null
);
