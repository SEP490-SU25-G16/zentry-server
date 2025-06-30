using SendGrid.Helpers.Errors.Model;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.CreateSchedule;

public class CreateScheduleCommandHandler(
    IScheduleRepository scheduleRepository,
    ICourseRepository courseRepository,
    IRoomRepository roomRepository,
    ILecturerLookupService lecturerLookupService)
    : ICommandHandler<CreateScheduleCommand, ScheduleCreatedResponseDto>
{
    public async Task<ScheduleCreatedResponseDto> Handle(CreateScheduleCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Basic Validation
        if (!command.IsValidTimeRange())
            // Sử dụng BusinessLogicException cho các lỗi nghiệp vụ
            throw new Exception("StartTime must be before EndTime.");

        // 2. Kiểm tra sự tồn tại của các Entities liên quan
        var courseExists = await courseRepository.GetByIdAsync(command.CourseId, cancellationToken);
        if (courseExists == null) throw new NotFoundException($"Course with ID '{command.CourseId}' not found.");

        var roomExists = await roomRepository.GetByIdAsync(command.RoomId, cancellationToken);
        if (roomExists == null) throw new NotFoundException($"Room with ID '{command.RoomId}' not found.");

        // Kiểm tra LecturerId bằng ILecturerLookupService
        var lecturerExists = await lecturerLookupService.GetLecturerByIdAsync(command.LecturerId, cancellationToken);
        if (lecturerExists == null)
            throw new NotFoundException(
                $"Lecturer with ID '{command.LecturerId}' not found or is not a valid lecturer.");


        // 3. Kiểm tra trùng lặp thời gian / khả dụng (Business Rules quan trọng)

        // Kiểm tra giảng viên có bận vào thời gian này không
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
        var responseDto = new ScheduleCreatedResponseDto
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
