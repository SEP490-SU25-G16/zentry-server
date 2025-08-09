using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Schedule;
using Zentry.SharedKernel.Contracts.Events;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ScheduleManagement.Application.Features.Schedules.UpdateSchedule;

public class UpdateScheduleCommandHandler(
    IScheduleRepository scheduleRepository,
    IRoomRepository roomRepository,
    IPublishEndpoint publishEndpoint,
    ILogger<UpdateScheduleCommandHandler> logger)
    : ICommandHandler<UpdateScheduleCommand, Unit>
{
    public async Task<Unit> Handle(UpdateScheduleCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to update schedule {ScheduleId}.", command.ScheduleId);

        // Lấy schedule cũ
        var schedule = await scheduleRepository.GetByIdAsync(command.ScheduleId, cancellationToken);
        if (schedule is null)
        {
            throw new ResourceNotFoundException("Schedule", $"ID '{command.ScheduleId}' not found.");
        }

        // Kiểm tra Room mới nếu có
        if (command.RoomId.HasValue)
        {
            var room = await roomRepository.GetByIdAsync(command.RoomId.Value, cancellationToken);
            if (room is null)
            {
                throw new ResourceNotFoundException("Room", $"ID '{command.RoomId.Value}' not found.");
            }
        }

        // Kiểm tra tính khả dụng của phòng mới trước khi cập nhật
        var newWeekDay = command.WeekDay != null ? WeekDayEnum.FromName(command.WeekDay) : schedule.WeekDay;
        var newStartTime = command.StartTime ?? schedule.StartTime;
        var newEndTime = command.EndTime ?? schedule.EndTime;
        var newStartDate = command.StartDate ?? schedule.StartDate;
        var newEndDate = command.EndDate ?? schedule.EndDate;
        var newRoomId = command.RoomId ?? schedule.RoomId;

        // Chỉ kiểm tra khi có sự thay đổi
        if (newRoomId != schedule.RoomId || newWeekDay != schedule.WeekDay || newStartTime != schedule.StartTime ||
            newEndTime != schedule.EndTime || newStartDate != schedule.StartDate || newEndDate != schedule.EndDate)
        {
            if (!await scheduleRepository.IsRoomAvailableForUpdateAsync(newRoomId, newWeekDay, newStartTime, newEndTime,
                    newStartDate, newEndDate, schedule.Id, cancellationToken))
            {
                throw new BusinessRuleException("ROOM_BOOKED",
                    $"Phòng đã được đặt vào {newWeekDay} từ {newStartTime} đến {newEndTime} trong khoảng thời gian này.");
            }
        }

        // Cập nhật schedule
        schedule.Update(
            roomId: command.RoomId,
            startDate: command.StartDate,
            endDate: command.EndDate,
            startTime: command.StartTime,
            endTime: command.EndTime,
            weekDay: command.WeekDay != null ? WeekDayEnum.FromName(command.WeekDay) : null
        );

        await scheduleRepository.UpdateAsync(schedule, cancellationToken);
        logger.LogInformation("Schedule {ScheduleId} updated successfully.", schedule.Id);

        var scheduleUpdatedEvent = new ScheduleUpdatedMessage(
            schedule.Id,
            schedule.StartTime,
            schedule.EndTime
        );
        await publishEndpoint.Publish(scheduleUpdatedEvent, cancellationToken);
        logger.LogInformation("ScheduleUpdatedMessage published for ScheduleId: {ScheduleId}.", schedule.Id);

        return Unit.Value;
    }
}
