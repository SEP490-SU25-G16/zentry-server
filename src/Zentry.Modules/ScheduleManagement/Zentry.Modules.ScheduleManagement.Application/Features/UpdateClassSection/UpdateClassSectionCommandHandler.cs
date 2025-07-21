using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ScheduleManagement.Application.Features.UpdateClassSection;

public class UpdateClassSectionCommandHandler(IClassSectionRepository repo)
    : ICommandHandler<UpdateClassSectionCommand, bool>
{
    public async Task<bool> Handle(UpdateClassSectionCommand command, CancellationToken cancellationToken)
    {
        var classSection = await repo.GetByIdAsync(command.Id, cancellationToken);

        if (classSection is null || classSection.IsDeleted)
            throw new NotFoundException("ClassSection", command.Id);

        classSection.Update(command.SectionCode, command.Semester);
        await repo.UpdateAsync(classSection, cancellationToken);
        await repo.SaveChangesAsync(cancellationToken);

        return true;
    }
}
