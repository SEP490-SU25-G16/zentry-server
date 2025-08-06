using Zentry.Modules.ScheduleManagement.Application.Features.GetClassSections;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.ScheduleManagement.Application.Abstractions;

public interface IClassSectionRepository : IRepository<ClassSection, Guid>
{
    Task<(List<ClassSection> Items, int TotalCount)> GetPagedClassSectionsAsync(
        ClassSectionListCriteria criteria,
        CancellationToken cancellationToken);

    Task<ClassSection?> GetBySectionCodeAsync(string sectionCode, string semester, CancellationToken cancellationToken);
    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken);
    public Task<List<ClassSection>> GetLecturerClassSectionsAsync(Guid lecturerId, CancellationToken cancellationToken);
    Task<ClassSection?> GetByScheduleIdAsync(Guid scheduleId, CancellationToken cancellationToken);
    Task<bool> IsExistClassSectionByCourseIdAsync(Guid courseId, CancellationToken cancellationToken);
}
