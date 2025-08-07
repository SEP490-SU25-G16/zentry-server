using Zentry.Modules.ScheduleManagement.Application.Features.GetClassSections;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.Modules.ScheduleManagement.Domain.ValueObjects;
using Zentry.SharedKernel.Abstractions.Data;

namespace Zentry.Modules.ScheduleManagement.Application.Abstractions;

public interface IClassSectionRepository : IRepository<ClassSection, Guid>
{
    Task<(List<ClassSection> Items, int TotalCount)> GetPagedClassSectionsAsync(
        ClassSectionListCriteria criteria,
        CancellationToken cancellationToken);

    Task<List<ClassSection>> GetBySectionCodesAsync(List<string> sectionCodes, CancellationToken cancellationToken);
    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken);
    public Task<List<ClassSection>> GetLecturerClassSectionsAsync(Guid lecturerId, CancellationToken cancellationToken);
    Task<ClassSection?> GetByScheduleIdAsync(Guid scheduleId, CancellationToken cancellationToken);
    Task<bool> IsExistClassSectionByCourseIdAsync(Guid courseId, CancellationToken cancellationToken);
    Task<bool> IsExistClassSectionBySectionCodeAsync(Guid id, string sectionCode, CancellationToken cancellationToken);

    Task<ClassSection?> GetBySectionCodeAndSemesterAsync(string sectionCode, Semester semester,
        CancellationToken cancellationToken = default);
}
