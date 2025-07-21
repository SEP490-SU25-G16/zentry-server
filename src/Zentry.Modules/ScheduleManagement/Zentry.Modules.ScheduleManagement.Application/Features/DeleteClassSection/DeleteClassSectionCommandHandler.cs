using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.DeleteClassSection;

public class DeleteClassSectionCommandHandler(IClassSectionRepository classSectionRepository)
    : ICommandHandler<DeleteClassSectionCommand, bool>
{
    public async Task<bool> Handle(DeleteClassSectionCommand command, CancellationToken cancellationToken)
    {
        var classSection = await classSectionRepository.GetByIdAsync(command.Id, cancellationToken);

        if (classSection is not null)
            throw new Exception($"Class with ID '{command.Id}' not found or already deleted.");

        await classSectionRepository.SoftDeleteAsync(command.Id, cancellationToken);
        await classSectionRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
