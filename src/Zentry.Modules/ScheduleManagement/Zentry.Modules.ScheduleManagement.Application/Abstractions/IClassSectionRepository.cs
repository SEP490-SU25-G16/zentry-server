using Zentry.Modules.ScheduleManagement.Application.Features.GetClassSections;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.ScheduleManagement.Application.Abstractions;

public interface IClassSectionRepository : IRepository<ClassSection, Guid>
{
    Task<(List<ClassSection> Items, int TotalCount)> GetPagedClassSectionsAsync(
        ClassSectionListCriteria criteria,
        CancellationToken cancellationToken);
}
