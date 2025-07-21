using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Services;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ScheduleManagement.Application.Features.CreateSchedule;

public class CreateScheduleCommandHandler(
    IScheduleRepository scheduleRepository,
    IClassSectionRepository classSectionRepository,
    IRoomRepository roomRepository,
    IUserScheduleService lecturerScheduleService)
    : ICommandHandler<CreateScheduleCommand, CreatedScheduleResponse>
{
    public async Task<CreatedScheduleResponse> Handle(CreateScheduleCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.IsValidTimeRange())
            throw new Exception("StartTime must be before EndTime.");

        var section = await classSectionRepository.GetByIdAsync(command.ClassSectionId, cancellationToken);
        if (section is null)
            throw new NotFoundException("ClassSection", $"ID '{command.ClassSectionId}' not found.");

        var room = await roomRepository.GetByIdAsync(command.RoomId, cancellationToken);
        if (room is null)
            throw new NotFoundException("Room", $"ID '{command.RoomId}' not found.");

        var lecturer =
            await lecturerScheduleService.GetUserByIdAndRoleAsync("Lecturer", command.LecturerId, cancellationToken);
        if (lecturer is null)
            throw new NotFoundException("Lecturer", $"ID '{command.LecturerId}' not found or invalid.");

        // Check if lecturer is free
        if (!await scheduleRepository.IsLecturerAvailableAsync(command.LecturerId, command.WeekDay, command.StartTime,
                command.EndTime, cancellationToken))
            throw new Exception(
                $"Lecturer {lecturer.FullName} is busy on {command.WeekDay} from {command.StartTime} to {command.EndTime}.");

        // Check if room is free
        if (!await scheduleRepository.IsRoomAvailableAsync(command.RoomId, command.WeekDay, command.StartTime,
                command.EndTime, cancellationToken))
            throw new Exception(
                $"Room is booked on {command.WeekDay} from {command.StartTime} to {command.EndTime}.");

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