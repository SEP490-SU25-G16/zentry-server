using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Contracts.Messages;

namespace Zentry.Modules.AttendanceManagement.Application.EventHandlers;

public class CreateRoundMessageConsumer(
    ILogger<CreateRoundMessageConsumer> logger,
    IServiceScopeFactory serviceScopeFactory)
    : IConsumer<CreateRoundMessage>
{
    public async Task Consume(ConsumeContext<CreateRoundMessage> consumeContext)
    {
        var message = consumeContext.Message;
        logger.LogInformation("MassTransit Consumer: Received request to create rounds for Session: {SessionId}. Total rounds: {TotalRounds}.",
            message.SessionId, message.TotalRoundsInSession);

        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var roundRepository = scope.ServiceProvider.GetRequiredService<IRoundRepository>();

            var totalDuration = message.ScheduledEndTime.Subtract(message.ScheduledStartTime);

            if (message.TotalRoundsInSession <= message.RoundNumber) // total rounds <= current round number, no more rounds to create
            {
                logger.LogInformation("No additional rounds to create for Session {SessionId}. Total rounds configured: {TotalRounds}, Current Round Number in message: {CurrentRound}.",
                    message.SessionId, message.TotalRoundsInSession, message.RoundNumber);
                return;
            }

            double durationPerRound = 0;
            if (message.TotalRoundsInSession > 0) // Defensive check
            {
                durationPerRound = totalDuration.TotalSeconds / message.TotalRoundsInSession;
            }

            var roundsToAdd = new List<Round>();

            // Bắt đầu từ round tiếp theo sau RoundNumber đã có (ví dụ: Round 2 nếu RoundNumber = 1)
            for (var i = message.RoundNumber + 1; i <= message.TotalRoundsInSession; i++)
            {
                var roundStartTime = message.ScheduledStartTime.AddSeconds(durationPerRound * (i - 1));

                var newRound = Round.Create(
                    message.SessionId,
                    i,
                    roundStartTime
                );
                roundsToAdd.Add(newRound);
            }

            if (roundsToAdd.Count > 0)
            {
                await roundRepository.AddRangeAsync(roundsToAdd, consumeContext.CancellationToken);
                await roundRepository.SaveChangesAsync(consumeContext.CancellationToken);
                logger.LogInformation("MassTransit Consumer: Successfully created and saved {NumRounds} additional rounds for Session {SessionId}.",
                    roundsToAdd.Count, message.SessionId);
            }
            else
            {
                logger.LogInformation("No additional rounds generated to add for Session {SessionId}.", message.SessionId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "MassTransit Consumer: Error creating rounds for Session {SessionId}. Message will be retried or moved to error queue.", message.SessionId);
            throw;
        }
    }
}
