namespace Zentry.SharedKernel.Contracts.Events;

public record GenerateSessionWhitelistMessage(
    Guid SessionId,
    Guid ScheduleId,
    Guid LecturerId,
    Guid ClassSectionId
);
