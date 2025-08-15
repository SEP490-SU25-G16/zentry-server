using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Contracts.Device;
using Zentry.SharedKernel.Contracts.Events;
using Zentry.SharedKernel.Contracts.Schedule;

namespace Zentry.Modules.AttendanceManagement.Application.EventHandlers;

public class GenerateScheduleWhitelistConsumer(
    ILogger<GenerateScheduleWhitelistConsumer> logger,
    IScheduleWhitelistRepository scheduleWhitelistRepository,
    IMediator mediator)
    : IConsumer<ScheduleCreatedMessage>, IConsumer<AssignLecturerToWhitelistMessage>
{
    public async Task Consume(ConsumeContext<AssignLecturerToWhitelistMessage> consumeContext)
    {
        var message = consumeContext.Message;
        logger.LogInformation(
            "MassTransit Consumer: Received request to assign lecturer {LecturerId} to whitelist for Schedule: {ScheduleId}.",
            message.LecturerId, message.ScheduleId);

        if (message.LecturerId == null || message.LecturerId.Value == Guid.Empty)
        {
            logger.LogWarning("LecturerId is null or empty. Skipping whitelist update for Schedule {ScheduleId}.",
                message.ScheduleId);
            return;
        }

        try
        {
            var existingWhitelist =
                await scheduleWhitelistRepository.GetByScheduleIdAsync(message.ScheduleId,
                    consumeContext.CancellationToken);

            if (existingWhitelist == null)
            {
                logger.LogWarning(
                    "Whitelist not found for Schedule {ScheduleId}. Cannot add lecturer. This might be an out-of-order event.",
                    message.ScheduleId);
                return;
            }

            var getLecturerDeviceQuery = new GetDeviceByUserIntegrationQuery(message.LecturerId.Value);
            var lecturerDeviceResponse = await mediator.Send(getLecturerDeviceQuery, consumeContext.CancellationToken);

            if (lecturerDeviceResponse.DeviceId == Guid.Empty)
            {
                logger.LogWarning("Lecturer {LecturerId} does not have an associated device. Whitelist not updated.",
                    message.LecturerId);
                return;
            }

            var whitelistedDeviceIds = new HashSet<Guid>(existingWhitelist.WhitelistedDeviceIds);

            if (whitelistedDeviceIds.Add(lecturerDeviceResponse.DeviceId))
            {
                existingWhitelist.UpdateWhitelist(whitelistedDeviceIds.ToList());
                await scheduleWhitelistRepository.UpdateAsync(existingWhitelist, consumeContext.CancellationToken);
                logger.LogInformation(
                    "Successfully added lecturer's device {DeviceId} to whitelist for Schedule {ScheduleId}.",
                    lecturerDeviceResponse.DeviceId, message.ScheduleId);
            }
            else
            {
                logger.LogInformation(
                    "Lecturer's device {DeviceId} for Schedule {ScheduleId} already exists in the whitelist. No update needed.",
                    lecturerDeviceResponse.DeviceId, message.ScheduleId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "MassTransit Consumer: Error assigning lecturer to whitelist for Schedule {ScheduleId}.",
                message.ScheduleId);
            throw;
        }
    }

    public async Task Consume(ConsumeContext<ScheduleCreatedMessage> consumeContext)
    {
        var message = consumeContext.Message;
        logger.LogInformation(
            "MassTransit Consumer: Received request to generate/update whitelist for Schedule: {ScheduleId}.",
            message.ScheduleId);

        try
        {
            var existingWhitelist =
                await scheduleWhitelistRepository.GetByScheduleIdAsync(message.ScheduleId,
                    consumeContext.CancellationToken);
            var whitelistedDeviceIds = new HashSet<Guid>();

            if (existingWhitelist != null)
            {
                whitelistedDeviceIds = new HashSet<Guid>(existingWhitelist.WhitelistedDeviceIds);
                logger.LogInformation("Whitelist already exists for Schedule {ScheduleId}. Adding lecturer device.",
                    message.ScheduleId);

                if (message.LecturerId.HasValue && message.LecturerId.Value != Guid.Empty)
                {
                    var getLecturerDeviceQuery = new GetDeviceByUserIntegrationQuery(message.LecturerId.Value);
                    var lecturerDeviceResponse =
                        await mediator.Send(getLecturerDeviceQuery, consumeContext.CancellationToken);
                    if (lecturerDeviceResponse.DeviceId != Guid.Empty)
                    {
                        whitelistedDeviceIds.Add(lecturerDeviceResponse.DeviceId);
                        logger.LogInformation("Added lecturer's device {DeviceId} to existing whitelist.",
                            lecturerDeviceResponse.DeviceId);
                    }
                }

                existingWhitelist.UpdateWhitelist(whitelistedDeviceIds.ToList());
                await scheduleWhitelistRepository.UpdateAsync(existingWhitelist, consumeContext.CancellationToken);
                logger.LogInformation(
                    "Successfully updated existing ScheduleWhitelist for Schedule {ScheduleId} with {DeviceCount} unique devices.",
                    message.ScheduleId, whitelistedDeviceIds.Count);
            }
            else
            {
                // Whitelist chưa tồn tại, tạo mới hoàn toàn.
                logger.LogInformation("Whitelist does not exist for Schedule {ScheduleId}. Creating a new one.",
                    message.ScheduleId);

                var getStudentIdsQuery = new GetStudentIdsByClassSectionIdIntegrationQuery(message.ClassSectionId);
                var studentIdsResponse = await mediator.Send(getStudentIdsQuery, consumeContext.CancellationToken);
                var enrolledStudentIds = studentIdsResponse.StudentIds;

                if (enrolledStudentIds.Any())
                {
                    var getStudentDevicesQuery = new GetDevicesByUsersIntegrationQuery(enrolledStudentIds);
                    var studentDevicesResponse =
                        await mediator.Send(getStudentDevicesQuery, consumeContext.CancellationToken);
                    foreach (var deviceId in studentDevicesResponse.UserDeviceMap.Values)
                        whitelistedDeviceIds.Add(deviceId);

                    logger.LogInformation("Added {Count} active devices from {StudentCount} enrolled students.",
                        studentDevicesResponse.UserDeviceMap.Count, enrolledStudentIds.Count);
                }

                if (message.LecturerId.HasValue && message.LecturerId.Value != Guid.Empty)
                {
                    var getLecturerDeviceQuery = new GetDeviceByUserIntegrationQuery(message.LecturerId.Value);
                    var lecturerDeviceResponse =
                        await mediator.Send(getLecturerDeviceQuery, consumeContext.CancellationToken);
                    if (lecturerDeviceResponse.DeviceId != Guid.Empty)
                        whitelistedDeviceIds.Add(lecturerDeviceResponse.DeviceId);

                    logger.LogInformation("Added lecturer's device to new whitelist.");
                }

                var newWhitelist = ScheduleWhitelist.Create(message.ScheduleId, whitelistedDeviceIds.ToList());
                await scheduleWhitelistRepository.AddAsync(newWhitelist, consumeContext.CancellationToken);
                logger.LogInformation(
                    "Successfully created new ScheduleWhitelist for Schedule {ScheduleId} with {DeviceCount} unique devices.",
                    message.ScheduleId, whitelistedDeviceIds.Count);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "MassTransit Consumer: Error generating/updating whitelist for Schedule {ScheduleId}.",
                message.ScheduleId);
            throw;
        }
    }
}
