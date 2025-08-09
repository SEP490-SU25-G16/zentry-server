using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ScheduleManagement.Application.Features.ClassSections.DeleteClassSection;

public class DeleteClassSectionCommandHandler(IClassSectionRepository classSectionRepository)
    : ICommandHandler<DeleteClassSectionCommand, bool>
{
    public async Task<bool> Handle(DeleteClassSectionCommand command, CancellationToken cancellationToken)
    {
        var classSection = await classSectionRepository.GetByIdAsync(command.Id, cancellationToken);

        if (classSection is null)
            throw new ResourceNotFoundException("CLASS SECTION", command.Id);

        if (classSection.Enrollments.Count != 0)
            throw new ResourceCannotBeDeletedException("Class section", command.Id);

        await classSectionRepository.SoftDeleteAsync(command.Id, cancellationToken);
        await classSectionRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}