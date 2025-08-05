using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Contracts.Device;
using Zentry.SharedKernel.Contracts.Events;
using Zentry.SharedKernel.Contracts.Schedule;
using Zentry.SharedKernel.Exceptions;

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
            "MassTransit Consumer: Received request to generate whitelist for Session: {SessionId}, ScheduleId: {ScheduleId}.",
            message.SessionId, message.ScheduleId);

        try
        {
            var existingWhitelist =
                await sessionWhitelistRepository.GetBySessionIdAsync(message.SessionId,
                    consumeContext.CancellationToken);
            if (existingWhitelist != null)
                logger.LogInformation("Whitelist already exists for Session {SessionId}. Updating existing whitelist.",
                    message.SessionId);

            // --- 1. Lấy danh sách các thiết bị được phép
            var whitelistedDeviceIds =
                new HashSet<Guid>();

            // Lấy StudentIds trong ClassSection
            var getStudentIdsQuery = new GetStudentIdsByClassSectionIdIntegrationQuery(message.ClassSectionId);
            var studentIdsResponse = await mediator.Send(getStudentIdsQuery, consumeContext.CancellationToken);
            var enrolledStudentIds = studentIdsResponse.StudentIds;

            if (enrolledStudentIds.Count != 0)
            {
                // Lấy tất cả active devices cho danh sách sinh viên đã đăng ký
                var getStudentDevicesQuery = new GetDevicesByUsersIntegrationQuery(enrolledStudentIds);
                var studentDevicesResponse =
                    await mediator.Send(getStudentDevicesQuery, consumeContext.CancellationToken);

                // Thêm tất cả DeviceId từ Dictionary của sinh viên vào HashSet
                foreach (var deviceId in studentDevicesResponse.UserDeviceMap.Values)
                    whitelistedDeviceIds.Add(deviceId);

                logger.LogInformation(
                    "Added {Count} active devices from {StudentCount} enrolled students to whitelist.",
                    studentDevicesResponse.UserDeviceMap.Count, enrolledStudentIds.Count);
            }
            else
            {
                logger.LogInformation(
                    "No enrolled students found for ClassSectionId {ClassSectionId}. No student devices added to whitelist.",
                    message.ClassSectionId);
            }

            // Chuyển đổi HashSet<Guid> thành List<Guid> để lưu vào SessionWhitelist
            var finalWhitelistedDevices = whitelistedDeviceIds.ToList();

            // --- 2. Tạo hoặc cập nhật SessionWhitelist và lưu vào DocumentDB ---
            if (existingWhitelist == null)
            {
                // Giả định SessionWhitelist.Create chấp nhận List<Guid>
                var newWhitelist = SessionWhitelist.Create(message.SessionId, finalWhitelistedDevices);
                await sessionWhitelistRepository.AddAsync(newWhitelist, consumeContext.CancellationToken);
                logger.LogInformation(
                    "Successfully created new SessionWhitelist for Session {SessionId} with {DeviceCount} unique devices.",
                    message.SessionId, finalWhitelistedDevices.Count);
            }
            else
            {
                // Giả định existingWhitelist.UpdateWhitelist chấp nhận List<Guid>
                existingWhitelist.UpdateWhitelist(finalWhitelistedDevices);
                await sessionWhitelistRepository.UpdateAsync(existingWhitelist, consumeContext.CancellationToken);
                logger.LogInformation(
                    "Successfully updated existing SessionWhitelist for Session {SessionId} with {DeviceCount} unique devices.",
                    message.SessionId, finalWhitelistedDevices.Count);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "MassTransit Consumer: Error generating whitelist for Session {SessionId}. Message will be retried or moved to error queue.",
                message.SessionId);
            throw;
        }
    }
}
