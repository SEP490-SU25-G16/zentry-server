using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Contracts.Device;
using Zentry.SharedKernel.Contracts.Events;
using Zentry.SharedKernel.Contracts.Schedule;

namespace Zentry.Modules.AttendanceManagement.Application.EventHandlers;

public class GenerateSessionWhitelistConsumer(
    ILogger<GenerateSessionWhitelistConsumer> logger,
    ISessionWhitelistRepository sessionWhitelistRepository,
    IMediator mediator)
    : IConsumer<GenerateSessionWhitelistMessage>
{
    public async Task Consume(ConsumeContext<GenerateSessionWhitelistMessage> consumeContext)
    {
        var message = consumeContext.Message;
        logger.LogInformation(
            "MassTransit Consumer: Received request to generate/update whitelist for Session: {SessionId}.",
            message.SessionId);

        try
        {
            var existingWhitelist = await sessionWhitelistRepository.GetBySessionIdAsync(message.SessionId, consumeContext.CancellationToken);
            var whitelistedDeviceIds = new HashSet<Guid>();

            if (existingWhitelist != null)
            {
                whitelistedDeviceIds = new HashSet<Guid>(existingWhitelist.WhitelistedDeviceIds);
                logger.LogInformation("Whitelist already exists for Session {SessionId}. Adding lecturer device.", message.SessionId);

                if (message.LecturerId.HasValue && message.LecturerId.Value != Guid.Empty)
                {
                    var getLecturerDeviceQuery = new GetDeviceByUserIntegrationQuery(message.LecturerId.Value);
                    var lecturerDeviceResponse = await mediator.Send(getLecturerDeviceQuery, consumeContext.CancellationToken);
                    if (lecturerDeviceResponse.DeviceId != Guid.Empty)
                    {
                        whitelistedDeviceIds.Add(lecturerDeviceResponse.DeviceId);
                        logger.LogInformation("Added lecturer's device {DeviceId} to existing whitelist.", lecturerDeviceResponse.DeviceId);
                    }
                }

                existingWhitelist.UpdateWhitelist(whitelistedDeviceIds.ToList());
                await sessionWhitelistRepository.UpdateAsync(existingWhitelist, consumeContext.CancellationToken);
                logger.LogInformation("Successfully updated existing SessionWhitelist for Session {SessionId} with {DeviceCount} unique devices.", message.SessionId, whitelistedDeviceIds.Count);
            }
            else
            {
                logger.LogInformation("Whitelist does not exist for Session {SessionId}. Creating a new one.", message.SessionId);

                var getStudentIdsQuery = new GetStudentIdsByClassSectionIdIntegrationQuery(message.ClassSectionId);
                var studentIdsResponse = await mediator.Send(getStudentIdsQuery, consumeContext.CancellationToken);
                var enrolledStudentIds = studentIdsResponse.StudentIds;

                if (enrolledStudentIds.Any())
                {
                    var getStudentDevicesQuery = new GetDevicesByUsersIntegrationQuery(enrolledStudentIds);
                    var studentDevicesResponse = await mediator.Send(getStudentDevicesQuery, consumeContext.CancellationToken);
                    foreach (var deviceId in studentDevicesResponse.UserDeviceMap.Values)
                    {
                        whitelistedDeviceIds.Add(deviceId);
                    }
                    logger.LogInformation("Added {Count} active devices from {StudentCount} enrolled students.", studentDevicesResponse.UserDeviceMap.Count, enrolledStudentIds.Count);
                }

                if (message.LecturerId.HasValue && message.LecturerId.Value != Guid.Empty)
                {
                    var getLecturerDeviceQuery = new GetDeviceByUserIntegrationQuery(message.LecturerId.Value);
                    var lecturerDeviceResponse = await mediator.Send(getLecturerDeviceQuery, consumeContext.CancellationToken);
                    if (lecturerDeviceResponse.DeviceId != Guid.Empty)
                    {
                        whitelistedDeviceIds.Add(lecturerDeviceResponse.DeviceId);
                    }
                    logger.LogInformation("Added lecturer's device to new whitelist.");
                }

                var newWhitelist = SessionWhitelist.Create(message.SessionId, whitelistedDeviceIds.ToList());
                await sessionWhitelistRepository.AddAsync(newWhitelist, consumeContext.CancellationToken);
                logger.LogInformation("Successfully created new SessionWhitelist for Session {SessionId} with {DeviceCount} unique devices.", message.SessionId, whitelistedDeviceIds.Count);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "MassTransit Consumer: Error generating/updating whitelist for Session {SessionId}.", message.SessionId);
            throw;
        }
    }
}
