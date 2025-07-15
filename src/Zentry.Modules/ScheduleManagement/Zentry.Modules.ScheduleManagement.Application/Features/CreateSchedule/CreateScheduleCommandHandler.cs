// using SendGrid.Helpers.Errors.Model;

using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ScheduleManagement.Application.Features.CreateSchedule;

public class CreateScheduleCommandHandler(
    IScheduleRepository scheduleRepository,
    ICourseRepository courseRepository,
    IRoomRepository roomRepository,
    IUserScheduleService lecturerScheduleService)
    : ICommandHandler<CreateScheduleCommand, CreatedScheduleResponse>
{
    public async Task<CreatedScheduleResponse> Handle(CreateScheduleCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.IsValidTimeRange())
            throw new Exception("StartTime must be before EndTime.");

        var courseExists = await courseRepository.GetByIdAsync(command.CourseId, cancellationToken);
        if (courseExists == null)
            throw new NotFoundException(nameof(ScheduleManagement), $"Course with ID '{command.CourseId}' not found.");

        var roomExists = await roomRepository.GetByIdAsync(command.RoomId, cancellationToken);
        if (roomExists == null)
            throw new NotFoundException(nameof(ScheduleManagement), $"Room with ID '{command.RoomId}' not found.");

        // Kiểm tra LecturerId bằng ILecturerLookupService
        var lecturerExists =
            await lecturerScheduleService.GetUserByIdAndRoleAsync("Lecturer", command.LecturerId, cancellationToken);
        if (lecturerExists == null)
            throw new NotFoundException(nameof(ScheduleManagement),
                $"Lecturer with ID '{command.LecturerId}' not found or is not a valid lecturer.");


        var isLecturerAvailable = await scheduleRepository.IsLecturerAvailableAsync(
            command.LecturerId,
            command.DayOfWeek,
            command.StartTime,
            command.EndTime,
            cancellationToken
        );
        if (!isLecturerAvailable)
            // Sử dụng BusinessLogicException và có thể dùng thông tin từ lecturerExists
            throw new Exception(
                $"Lecturer {lecturerExists.FullName} is already scheduled for another class on {command.DayOfWeek} from {command.StartTime.ToShortTimeString()} to {command.EndTime.ToShortTimeString()}.");

        // Kiểm tra phòng học có trống vào thời gian này không
        var isRoomAvailable = await scheduleRepository.IsRoomAvailableAsync(
            command.RoomId,
            command.DayOfWeek,
            command.StartTime,
            command.EndTime,
            cancellationToken
        );
        if (!isRoomAvailable)
            throw new Exception(
                $"Room is already booked on {command.DayOfWeek} from {command.StartTime.ToShortTimeString()} to {command.EndTime.ToShortTimeString()}.");

        // 4. Tạo đối tượng Schedule Domain Entity
        var schedule = Schedule.Create(
            command.LecturerId,
            command.CourseId,
            command.RoomId,
            command.StartTime,
            command.EndTime,
            command.DayOfWeek
        );

        // 5. Lưu vào cơ sở dữ liệu thông qua Repository
        await scheduleRepository.AddAsync(schedule, cancellationToken);
        await scheduleRepository.SaveChangesAsync(cancellationToken);

        // 6. Ánh xạ từ Domain Entity sang DTO để trả về
        var responseDto = new CreatedScheduleResponse
        {
            Id = schedule.Id,
            LecturerId = schedule.LecturerId,
            CourseId = schedule.CourseId,
            RoomId = schedule.RoomId,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            DayOfWeek = schedule.DayOfWeek,
            CreatedAt = schedule.CreatedAt
        };

        return responseDto;
    }
}