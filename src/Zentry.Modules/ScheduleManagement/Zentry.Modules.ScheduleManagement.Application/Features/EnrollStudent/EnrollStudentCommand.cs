using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.EnrollStudent;

public class EnrollStudentCommand : ICommand<EnrollmentResponse>
{
    // AdminId sẽ được Controller gán từ JWT, không phải từ request body.
    public Guid AdminId { get; set; } // UserId từ JWT của Admin

    public Guid ScheduleId { get; set; } // Changed from CourseId to ScheduleId
    public Guid StudentId { get; set; }
}
