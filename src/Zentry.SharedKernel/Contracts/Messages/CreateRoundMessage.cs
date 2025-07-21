namespace Zentry.SharedKernel.Contracts.Messages;

public record CreateRoundMessage(
    Guid SessionId,
    int RoundNumber,
    int TotalRoundsInSession,
    DateTime ScheduledStartTime,
    DateTime ScheduledEndTime
);