using MediatR;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ScheduleManagement.Application.Features.EnrollStudent;

public class EnrollStudentCommandHandler(
    IEnrollmentRepository enrollmentRepository,
    IScheduleRepository scheduleRepository,
    IUserScheduleService userLookupService,
    IMediator mediator)
    : ICommandHandler<EnrollStudentCommand, EnrollmentResponse>
{
    private readonly IMediator _mediator = mediator;

    public async Task<EnrollmentResponse> Handle(EnrollStudentCommand command, CancellationToken cancellationToken)
    {
        var studentUser =
            await userLookupService.GetUserByIdAndRoleAsync("student", command.StudentId, cancellationToken);
        if (studentUser == null)
            throw new NotFoundException("Student", command.StudentId); // Lỗi 404

        var alreadyEnrolled =
            await enrollmentRepository.ExistsAsync(command.StudentId, command.ScheduleId, cancellationToken);
        if (alreadyEnrolled) throw new BusinessLogicException("Student already enrolled in this schedule."); // Lỗi 400

        var enrollment = Enrollment.Create(command.StudentId, command.ScheduleId);

        await enrollmentRepository.AddAsync(enrollment, cancellationToken);

        await enrollmentRepository.SaveChangesAsync(cancellationToken);

        // await _mediator.Publish(new StudentEnrolledEvent(enrollment.Id, enrollment.StudentId, enrollment.ScheduleId), cancellationToken);
        // Logger.LogInformation($"Student {command.StudentId} enrolled in Schedule {command.ScheduleId} by Admin {command.AdminId}");


        return new EnrollmentResponse
        {
            EnrollmentId = enrollment.Id,
            ScheduleId = enrollment.ScheduleId,
            StudentId = enrollment.StudentId,
            StudentName = studentUser.FullName ?? "Unknown Student",
            EnrollmentDate = enrollment.EnrolledAt,
            Status = enrollment.Status.ToString()
        };
    }
}
