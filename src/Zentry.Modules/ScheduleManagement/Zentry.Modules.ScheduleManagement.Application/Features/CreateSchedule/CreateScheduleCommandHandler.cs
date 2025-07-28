using MassTransit;
using Microsoft.Extensions.Logging;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Services;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.User;
using Zentry.SharedKernel.Contracts.Events;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ScheduleManagement.Application.Features.CreateSchedule;

public class CreateScheduleCommandHandler(
    IScheduleRepository scheduleRepository,
    IClassSectionRepository classSectionRepository,
    IRoomRepository roomRepository,
    IUserScheduleService lecturerScheduleService,
    IPublishEndpoint publishEndpoint,
    ILogger<CreateScheduleCommandHandler> logger)
    : ICommandHandler<CreateScheduleCommand, CreatedScheduleResponse>
{
    public async Task<CreatedScheduleResponse> Handle(CreateScheduleCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.IsValidTimeRange())
            throw new BusinessRuleException("INVALID_TIME_RANGE", "Thời gian bắt đầu phải trước thời gian kết thúc.");

        var section = await classSectionRepository.GetByIdAsync(command.ClassSectionId, cancellationToken);
        if (section is null)
            throw new NotFoundException("ClassSection", $"ID '{command.ClassSectionId}' not found.");

        var room = await roomRepository.GetByIdAsync(command.RoomId, cancellationToken);
        if (room is null)
            throw new NotFoundException("Room", $"ID '{command.RoomId}' not found.");

        var lecturer =
            await lecturerScheduleService.GetUserByIdAndRoleAsync(Role.Lecturer, command.LecturerId, cancellationToken);
        if (lecturer is null)
            throw new NotFoundException(Role.Lecturer.ToString(), $"ID '{command.LecturerId}' not found or invalid.");

        if (!await scheduleRepository.IsLecturerAvailableAsync(command.LecturerId, command.WeekDay, command.StartTime,
                command.EndTime, cancellationToken))
            throw new BusinessRuleException("LECTURER_BUSY",
                $"Giảng viên {lecturer.FullName} đã bận vào {command.WeekDay} từ {command.StartTime} đến {command.EndTime}.");

        if (!await scheduleRepository.IsRoomAvailableAsync(command.RoomId, command.WeekDay, command.StartTime,
                command.EndTime, cancellationToken))
            throw new BusinessRuleException("ROOM_BOOKED",
                $"Phòng đã được đặt vào {command.WeekDay} từ {command.StartTime} đến {command.EndTime}.");

        var schedule = Schedule.Create(
            section.Id,
            command.RoomId,
            weekDay: command.WeekDay,
            startTime: command.StartTime,
            endTime: command.EndTime,
            startDate: command.StartDate,
            endDate: command.EndDate
        );

        await scheduleRepository.AddAsync(schedule, cancellationToken);
        await scheduleRepository.SaveChangesAsync(cancellationToken);

        var scheduleCreatedEvent = new CreateSesssionMessage(
            schedule.Id,
            command.LecturerId,
            schedule.ClassSectionId,
            schedule.RoomId,
            schedule.WeekDay.ToString(),
            schedule.StartTime,
            schedule.EndTime,
            schedule.StartDate,
            schedule.EndDate,
            section.CourseId,
            schedule.CreatedAt
        );

        await publishEndpoint.Publish(scheduleCreatedEvent, cancellationToken);
        logger.LogInformation("ScheduleCreatedEvent published for ScheduleId: {ScheduleId}.", schedule.Id);

        return new CreatedScheduleResponse
        {
            Id = schedule.Id,
            LecturerId = command.LecturerId,
            ClassSectionId = schedule.ClassSectionId,
            RoomId = schedule.RoomId,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            WeekDay = schedule.WeekDay,
            StartDate = schedule.StartDate,
            EndDate = schedule.EndDate,
            CreatedAt = schedule.CreatedAt
        };
    }
}
