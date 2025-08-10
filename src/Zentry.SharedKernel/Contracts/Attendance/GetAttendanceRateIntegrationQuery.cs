using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.SharedKernel.Contracts.Attendance;

public record GetAttendanceRateIntegrationQuery(Guid ClassSectionId)
    : IQuery<double>;
