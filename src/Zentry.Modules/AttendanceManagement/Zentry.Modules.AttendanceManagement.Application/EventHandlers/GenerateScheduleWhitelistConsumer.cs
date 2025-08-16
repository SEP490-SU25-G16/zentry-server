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

            // Lấy device của lecturer
            var getLecturerDeviceQuery = new GetDeviceByUserIntegrationQuery(message.LecturerId.Value);
            var lecturerDeviceResponse = await mediator.Send(getLecturerDeviceQuery, consumeContext.CancellationToken);

            if (lecturerDeviceResponse.DeviceId == Guid.Empty)
            {
                logger.LogWarning("Lecturer {LecturerId} does not have an associated device. Whitelist not updated.",
                    message.LecturerId);
                return;
            }

            if (existingWhitelist != null)
            {
                // Whitelist đã tồn tại, chỉ cần thêm lecturer device
                var whitelistedDeviceIds = new HashSet<Guid>(existingWhitelist.WhitelistedDeviceIds);

                if (whitelistedDeviceIds.Add(lecturerDeviceResponse.DeviceId))
                {
                    existingWhitelist.UpdateWhitelist(whitelistedDeviceIds.ToList());
                    await scheduleWhitelistRepository.UpdateAsync(existingWhitelist, consumeContext.CancellationToken);
                    logger.LogInformation(
                        "Successfully added lecturer's device {DeviceId} to existing whitelist for Schedule {ScheduleId}.",
                        lecturerDeviceResponse.DeviceId, message.ScheduleId);
                }
                else
                {
                    logger.LogInformation(
                        "Lecturer's device {DeviceId} for Schedule {ScheduleId} already exists in the whitelist. No update needed.",
                        lecturerDeviceResponse.DeviceId, message.ScheduleId);
                }
            }
            else
            {
                // Whitelist chưa tồn tại, tạo mới với chỉ lecturer device
                logger.LogInformation(
                    "Whitelist does not exist for Schedule {ScheduleId}. Creating a new whitelist with lecturer's device only.",
                    message.ScheduleId);

                var whitelistedDeviceIds = new List<Guid> { lecturerDeviceResponse.DeviceId };
                var newWhitelist = ScheduleWhitelist.Create(message.ScheduleId, whitelistedDeviceIds);
                await scheduleWhitelistRepository.AddAsync(newWhitelist, consumeContext.CancellationToken);

                logger.LogInformation(
                    "Successfully created new ScheduleWhitelist for Schedule {ScheduleId} with lecturer's device {DeviceId}.",
                    message.ScheduleId, lecturerDeviceResponse.DeviceId);
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

            if (existingWhitelist != null)
            {
                // Whitelist đã tồn tại, chỉ cần thêm student devices
                logger.LogInformation("Whitelist already exists for Schedule {ScheduleId}. Adding student devices only.",
                    message.ScheduleId);

                var whitelistedDeviceIds = new HashSet<Guid>(existingWhitelist.WhitelistedDeviceIds);

                // Lấy danh sách student devices và thêm vào
                var getStudentIdsQuery = new GetStudentIdsByClassSectionIdIntegrationQuery(message.ClassSectionId);
                var studentIdsResponse = await mediator.Send(getStudentIdsQuery, consumeContext.CancellationToken);
                var enrolledStudentIds = studentIdsResponse.StudentIds;

                if (enrolledStudentIds.Any())
                {
                    var getStudentDevicesQuery = new GetDevicesByUsersIntegrationQuery(enrolledStudentIds);
                    var studentDevicesResponse =
                        await mediator.Send(getStudentDevicesQuery, consumeContext.CancellationToken);

                    var addedDevices = 0;
                    foreach (var deviceId in studentDevicesResponse.UserDeviceMap.Values)
                    {
                        if (whitelistedDeviceIds.Add(deviceId))
                        {
                            addedDevices++;
                        }
                    }

                    logger.LogInformation("Added {AddedCount} new student devices from {StudentCount} enrolled students.",
                        addedDevices, enrolledStudentIds.Count);
                }

                existingWhitelist.UpdateWhitelist(whitelistedDeviceIds.ToList());
                await scheduleWhitelistRepository.UpdateAsync(existingWhitelist, consumeContext.CancellationToken);
                logger.LogInformation(
                    "Successfully updated existing ScheduleWhitelist for Schedule {ScheduleId} with {DeviceCount} total devices.",
                    message.ScheduleId, whitelistedDeviceIds.Count);
            }
            else
            {
                // Whitelist chưa tồn tại, tạo mới với students và lecturer (nếu có)
                logger.LogInformation("Whitelist does not exist for Schedule {ScheduleId}. Creating a new one.",
                    message.ScheduleId);

                var whitelistedDeviceIds = new HashSet<Guid>();

                // Thêm student devices
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

                    logger.LogInformation("Added {Count} student devices from {StudentCount} enrolled students.",
                        studentDevicesResponse.UserDeviceMap.Count, enrolledStudentIds.Count);
                }

                // Thêm lecturer device nếu có
                if (message.LecturerId.HasValue && message.LecturerId.Value != Guid.Empty)
                {
                    var getLecturerDeviceQuery = new GetDeviceByUserIntegrationQuery(message.LecturerId.Value);
                    var lecturerDeviceResponse =
                        await mediator.Send(getLecturerDeviceQuery, consumeContext.CancellationToken);
                    if (lecturerDeviceResponse.DeviceId != Guid.Empty)
                    {
                        whitelistedDeviceIds.Add(lecturerDeviceResponse.DeviceId);
                        logger.LogInformation("Added lecturer's device to new whitelist.");
                    }
                }

                var newWhitelist = ScheduleWhitelist.Create(message.ScheduleId, whitelistedDeviceIds.ToList());
                await scheduleWhitelistRepository.AddAsync(newWhitelist, consumeContext.CancellationToken);
                logger.LogInformation(
                    "Successfully created new ScheduleWhitelist for Schedule {ScheduleId} with {DeviceCount} total devices.",
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
