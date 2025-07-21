using Microsoft.EntityFrameworkCore;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Features.GetClassSections;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.Modules.ScheduleManagement.Infrastructure.Persistence;

namespace Zentry.Modules.ScheduleManagement.Infrastructure.Repositories;

public class ClassSectionRepository(ScheduleDbContext dbContext) : IClassSectionRepository
{
    public async Task<IEnumerable<ClassSection>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.ClassSections
            .Include(cs => cs.Course)
            .ToListAsync(cancellationToken);
    }

    public async Task<ClassSection?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.ClassSections
            .Include(cs => cs.Course)
            .FirstOrDefaultAsync(cs => cs.Id == id, cancellationToken);
    }

    public async Task AddAsync(ClassSection entity, CancellationToken cancellationToken)
    {
        await dbContext.ClassSections.AddAsync(entity, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<ClassSection> entities, CancellationToken cancellationToken)
    {
        await dbContext.ClassSections.AddRangeAsync(entities, cancellationToken);
    }

    public async Task UpdateAsync(ClassSection entity, CancellationToken cancellationToken)
    {
        dbContext.ClassSections.Update(entity);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ClassSection entity, CancellationToken cancellationToken)
    {
        dbContext.ClassSections.Remove(entity);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<(List<ClassSection> Items, int TotalCount)> GetPagedClassSectionsAsync(
        ClassSectionListCriteria criteria,
        CancellationToken cancellationToken)
    {
        var query = dbContext.ClassSections
            .Include(cs => cs.Course)
            .AsQueryable();

        if (criteria.CourseId.HasValue)
            query = query.Where(cs => cs.CourseId == criteria.CourseId.Value);

        if (criteria.LecturerId.HasValue)
            query = query.Where(cs => cs.LecturerId == criteria.LecturerId.Value);

        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            var lower = criteria.SearchTerm.ToLower();
            query = query.Where(cs =>
                cs.SectionCode.ToLower().Contains(lower) ||
                cs.Course!.Name.ToLower().Contains(lower) ||
                cs.Course.Code.ToLower().Contains(lower));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(criteria.SortBy))
        {
            query = criteria.SortBy.ToLower() switch
            {
                "sectioncode" => criteria.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(cs => cs.SectionCode)
                    : query.OrderBy(cs => cs.SectionCode),
                "coursename" => criteria.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(cs => cs.Course!.Name)
                    : query.OrderBy(cs => cs.Course!.Name),
                _ => query.OrderBy(cs => cs.SectionCode)
            };
        }
        else
        {
            query = query.OrderBy(cs => cs.SectionCode);
        }

        var items = await query
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
