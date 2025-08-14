using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ScheduleManagement.Application.Features.ClassSections.DeleteClassSection;

public class DeleteClassSectionCommandHandler(
    IClassSectionRepository classSectionRepository,
    IScheduleRepository scheduleRepository)
    : ICommandHandler<DeleteClassSectionCommand, bool>
{
    public async Task<bool> Handle(DeleteClassSectionCommand command, CancellationToken cancellationToken)
    {
        var classSection = await classSectionRepository.GetByIdAsync(command.Id, cancellationToken);

        if (classSection is null) throw new ResourceNotFoundException("CLASS SECTION", command.Id);

        if (classSection.Enrollments.Count != 0)
        {
            var hasActiveSchedule =
                await scheduleRepository.HasActiveScheduleByClassSectionIdAsync(classSection.Id, cancellationToken);

            if (hasActiveSchedule)
                throw new ResourceCannotBeDeletedException(
                    $"Class section with ID '{command.Id}' has active enrollments and a running schedule, so it cannot be deleted.");

            await classSectionRepository.SoftDeleteAsync(command.Id, cancellationToken);
        }

        await classSectionRepository.DeleteAsync(classSection, cancellationToken);
        return true;
    }
}