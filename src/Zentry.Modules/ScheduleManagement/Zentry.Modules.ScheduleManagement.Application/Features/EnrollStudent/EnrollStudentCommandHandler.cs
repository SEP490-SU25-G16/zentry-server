using MediatR;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Services;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ScheduleManagement.Application.Features.EnrollStudent;

public class EnrollStudentCommandHandler(
    IEnrollmentRepository enrollmentRepository,
    IClassSectionRepository classSectionRepository,
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
            throw new NotFoundException("Student", command.StudentId);

        var classSection = await classSectionRepository.GetByIdAsync(command.ClassSectionId, cancellationToken);
        if (classSection is null)
            throw new NotFoundException("ClassSection", command.ClassSectionId);

        var alreadyEnrolled =
            await enrollmentRepository.ExistsAsync(command.StudentId, command.ClassSectionId, cancellationToken);
        if (alreadyEnrolled)
            throw new BusinessLogicException("Student already enrolled in this class section.");

        var enrollment = Enrollment.Create(command.StudentId, command.ClassSectionId);

        await enrollmentRepository.AddAsync(enrollment, cancellationToken);
        await enrollmentRepository.SaveChangesAsync(cancellationToken);

        return new EnrollmentResponse
        {
            EnrollmentId = enrollment.Id,
            ClassSectionId = enrollment.ClassSectionId,
            StudentId = enrollment.StudentId,
            StudentName = studentUser.FullName ?? "Unknown Student",
            EnrollmentDate = enrollment.EnrolledAt,
            Status = enrollment.Status.ToString()
        };
    }
}
