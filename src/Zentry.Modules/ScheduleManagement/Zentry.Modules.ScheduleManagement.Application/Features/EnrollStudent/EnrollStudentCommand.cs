using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.EnrollStudent;

public class EnrollStudentCommand : ICommand<EnrollmentResponse>
{
    public Guid AdminId { get; set; }
    public Guid ScheduleId { get; set; }
    public Guid StudentId { get; set; }
}
