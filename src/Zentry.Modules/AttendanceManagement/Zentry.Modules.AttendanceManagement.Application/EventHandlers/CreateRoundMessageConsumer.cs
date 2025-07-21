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
        logger.LogInformation(
            "MassTransit Consumer: Received request to create rounds for Session: {SessionId}. Total rounds: {TotalRounds}.",
            message.SessionId, message.TotalRoundsInSession);

        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var roundRepository = scope.ServiceProvider.GetRequiredService<IRoundRepository>();

            var totalDuration = message.ScheduledEndTime.Subtract(message.ScheduledStartTime);

            // Kiểm tra số round cần tạo còn lại. message.RoundNumber là số round đã được tạo (ví dụ: round 1)
            if (message.TotalRoundsInSession <= message.RoundNumber)
            {
                logger.LogInformation(
                    "No additional rounds to create for Session {SessionId}. Total rounds configured: {TotalRounds}, Current Round Number in message: {CurrentRound}.",
                    message.SessionId, message.TotalRoundsInSession, message.RoundNumber);
                return;
            }

            double durationPerRoundSeconds = 0;
            if (message.TotalRoundsInSession > 0)
                durationPerRoundSeconds = totalDuration.TotalSeconds / message.TotalRoundsInSession;

            var roundsToAdd = new List<Round>();

            // Bắt đầu từ round tiếp theo sau RoundNumber đã có (ví dụ: Round 2 nếu RoundNumber = 1)
            for (var i = message.RoundNumber + 1; i <= message.TotalRoundsInSession; i++)
            {
                // Tính toán StartTime và EndTime cho round hiện tại
                // StartTime của Round i = ScheduledStartTime + (độ dài mỗi round * (i - 1))
                var roundStartTime = message.ScheduledStartTime.AddSeconds(durationPerRoundSeconds * (i - 1));
                // EndTime của Round i = StartTime của Round i + độ dài mỗi round
                var roundEndTime = roundStartTime.AddSeconds(durationPerRoundSeconds);

                // Đảm bảo EndTime của round cuối cùng không vượt quá ScheduledEndTime của session
                if (i == message.TotalRoundsInSession) roundEndTime = message.ScheduledEndTime;

                var newRound = Round.Create(
                    message.SessionId,
                    i,
                    roundStartTime,
                    roundEndTime // Truyền EndTime vào hàm tạo
                );
                roundsToAdd.Add(newRound);
            }

            if (roundsToAdd.Count > 0)
            {
                await roundRepository.AddRangeAsync(roundsToAdd, consumeContext.CancellationToken);
                await roundRepository.SaveChangesAsync(consumeContext.CancellationToken);
                logger.LogInformation(
                    "MassTransit Consumer: Successfully created and saved {NumRounds} additional rounds for Session {SessionId}.",
                    roundsToAdd.Count, message.SessionId);
            }
            else
            {
                logger.LogInformation("No additional rounds generated to add for Session {SessionId}.",
                    message.SessionId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "MassTransit Consumer: Error creating rounds for Session {SessionId}. Message will be retried or moved to error queue.",
                ex.Message); // Log chi tiết hơn
            throw;
        }
    }
}