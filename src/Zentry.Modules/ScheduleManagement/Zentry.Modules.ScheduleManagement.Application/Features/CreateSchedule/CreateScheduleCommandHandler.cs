using Zentry.Modules.ScheduleManagement.Application.Abstractions;
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

        if (!await scheduleRepository.IsLecturerAvailableAsync(command.LecturerId, command.DayOfWeek, command.StartTime,
                command.EndTime, cancellationToken))
            throw new Exception(
                $"Lecturer {lecturer.FullName} is busy on {command.DayOfWeek} from {command.StartTime:t} to {command.EndTime:t}.");

        if (!await scheduleRepository.IsRoomAvailableAsync(command.RoomId, command.DayOfWeek, command.StartTime,
                command.EndTime, cancellationToken))
            throw new Exception(
                $"Room is booked on {command.DayOfWeek} from {command.StartTime:t} to {command.EndTime:t}.");

        var schedule = Schedule.Create(
            section.Id,
            command.RoomId,
            command.StartTime,
            command.EndTime,
            command.DayOfWeek);

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
            DayOfWeek = schedule.DayOfWeek,
            CreatedAt = schedule.CreatedAt
        };
    }
}
