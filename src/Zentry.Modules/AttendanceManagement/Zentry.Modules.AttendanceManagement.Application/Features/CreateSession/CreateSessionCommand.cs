using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.AttendanceManagement.Application.Features.CreateSession;

public record CreateSessionCommand(
    Guid ScheduleId,
    Guid UserId, // ID của giảng viên
    DateTime StartTime,
    DateTime EndTime
) : ICommand<CreateSessionResponse>;
