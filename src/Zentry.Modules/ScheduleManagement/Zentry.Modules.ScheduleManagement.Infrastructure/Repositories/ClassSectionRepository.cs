using Microsoft.EntityFrameworkCore;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Features.GetClassSections;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.Modules.ScheduleManagement.Infrastructure.Persistence;

namespace Zentry.Modules.ScheduleManagement.Infrastructure.Repositories;

public class ClassSectionRepository(ScheduleDbContext dbContext) : IClassSectionRepository
{
    public async Task<ClassSection?> GetByScheduleIdAsync(Guid scheduleId, CancellationToken cancellationToken)
    {
        return await dbContext.ClassSections
            .Include(cs => cs.Course)
            .FirstOrDefaultAsync(cs => cs.Schedules.Any(s => s.Id == scheduleId), cancellationToken);
    }

    public async Task<List<ClassSection>> GetLecturerClassSectionsAsync(Guid lecturerId,
        CancellationToken cancellationToken)
    {
        return await dbContext.ClassSections
            .Include(cs => cs.Course)
            .Include(cs => cs.Schedules)
            .ThenInclude(s => s.Room)
            .Include(cs => cs.Enrollments)
            .Where(cs => cs.LecturerId == lecturerId)
            .ToListAsync(cancellationToken);
    }

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
            .Include(cs => cs.Schedules)
            .ThenInclude(s => s.Room)
            .Include(cs => cs.Enrollments)
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

        // Nếu có lọc theo StudentId, cần include Enrollments
        if (criteria.StudentId.HasValue && criteria.StudentId.Value != Guid.Empty)
            query = query.Include(cs => cs.Enrollments);

        // Lọc theo CourseId
        if (criteria.CourseId.HasValue && criteria.CourseId.Value != Guid.Empty)
            query = query.Where(cs => cs.CourseId == criteria.CourseId.Value);

        // Lọc theo LecturerId
        if (criteria.LecturerId.HasValue && criteria.LecturerId.Value != Guid.Empty)
            query = query.Where(cs => cs.LecturerId == criteria.LecturerId.Value);

        // Lọc theo StudentId
        if (criteria.StudentId.HasValue && criteria.StudentId.Value != Guid.Empty)
            query = query.Where(cs => cs.Enrollments.Any(e => e.StudentId == criteria.StudentId.Value));

        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            var lower = criteria.SearchTerm.ToLower();
            query = query.Where(cs =>
                cs.SectionCode.ToLower().Contains(lower) ||
                (cs.Course != null && cs.Course.Name.ToLower().Contains(lower)) || // Thêm kiểm tra null
                (cs.Course != null && cs.Course.Code.ToLower().Contains(lower))); // Thêm kiểm tra null
        }

        // Cập nhật để chỉ lấy các ClassSection chưa bị xóa mềm
        query = query.Where(cs => !cs.IsDeleted);

        var totalCount = await query.CountAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(criteria.SortBy))
            query = criteria.SortBy.ToLower() switch
            {
                "sectioncode" => criteria.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(cs => cs.SectionCode)
                    : query.OrderBy(cs => cs.SectionCode),
                "coursename" => criteria.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(cs => cs.Course!.Name) // Vẫn có thể null nếu Course không được Include
                    : query.OrderBy(cs => cs.Course!.Name),
                _ => query.OrderBy(cs => cs.SectionCode)
            };
        else
            query = query.OrderBy(cs => cs.SectionCode);

        var items = await query
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<ClassSection?> GetBySectionCodeAsync(string sectionCode, string semester,
        CancellationToken cancellationToken)
    {
        return await dbContext.ClassSections
            .Include(cs => cs.Course)
            .FirstOrDefaultAsync(cs =>
                    cs.SectionCode == sectionCode &&
                    cs.Semester == semester,
                cancellationToken);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var classSection = await dbContext.ClassSections.FindAsync([id], cancellationToken);

        if (classSection is not null)
        {
            classSection.Delete();
            dbContext.ClassSections.Update(classSection);
            await SaveChangesAsync(cancellationToken);
        }
    }
}