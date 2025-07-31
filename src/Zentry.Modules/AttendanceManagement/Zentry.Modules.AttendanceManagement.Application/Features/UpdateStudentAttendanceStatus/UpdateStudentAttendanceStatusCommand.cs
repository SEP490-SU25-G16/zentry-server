using MediatR;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Attendance;

namespace Zentry.Modules.AttendanceManagement.Application.Features.UpdateStudentAttendanceStatus;

public record UpdateStudentAttendanceStatusCommand(
    Guid SessionId,
    Guid UserId
) : ICommand<Unit>;
