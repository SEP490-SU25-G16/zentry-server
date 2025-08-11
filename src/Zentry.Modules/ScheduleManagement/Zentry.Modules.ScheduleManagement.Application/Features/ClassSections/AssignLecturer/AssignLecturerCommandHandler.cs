using MassTransit;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Events;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ScheduleManagement.Application.Features.ClassSections.AssignLecturer;

public class AssignLecturerCommandHandler(
    IScheduleRepository scheduleRepository,
    IClassSectionRepository classSectionRepository,
    IPublishEndpoint publishEndpoint)
    : ICommandHandler<AssignLecturerCommand, AssignLecturerResponse>
{
    public async Task<AssignLecturerResponse> Handle(AssignLecturerCommand command, CancellationToken cancellationToken)
    {
        var classSection = await classSectionRepository.GetByIdAsync(command.ClassSectionId, cancellationToken);
        if (classSection is null) throw new NotFoundException("ClassSection", command.ClassSectionId);

        if (classSection.LecturerId.HasValue)
        {
            var hasActiveSchedule =
                await scheduleRepository.HasActiveScheduleByClassSectionIdAsync(classSection.Id, cancellationToken);

            if (hasActiveSchedule)
            {
                throw new ScheduleConflictException(
                    $"Class section with ID '{command.ClassSectionId}' can not be updated because it  is already active.");
            }
        }

        classSection.AssignLecturer(command.LecturerId);
        await classSectionRepository.UpdateAsync(classSection, cancellationToken);
        await classSectionRepository.SaveChangesAsync(cancellationToken);

        var message = new AssignLecturerMessage(
            classSection.Id,
            command.LecturerId);

        await publishEndpoint.Publish(message, cancellationToken);

        return new AssignLecturerResponse(
            classSection.Id,
            command.LecturerId,
            true
        );
    }
}
