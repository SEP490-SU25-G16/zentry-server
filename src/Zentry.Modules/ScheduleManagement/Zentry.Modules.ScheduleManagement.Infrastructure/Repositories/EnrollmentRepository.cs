using Microsoft.EntityFrameworkCore;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Features.GetEnrollments;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.Modules.ScheduleManagement.Domain.Enums;
using Zentry.Modules.ScheduleManagement.Infrastructure.Persistence;

namespace Zentry.Modules.ScheduleManagement.Infrastructure.Repositories;

public class EnrollmentRepository(ScheduleDbContext dbContext) : IEnrollmentRepository
{
    public async Task<bool> ExistsAsync(Guid studentId, Guid scheduleId, CancellationToken cancellationToken)
    {
        return await dbContext.Enrollments
            .AsNoTracking()
            .AnyAsync(
                e => e.StudentId == studentId && e.ScheduleId == scheduleId && e.Status == EnrollmentStatus.Active,
                cancellationToken);
    }

    public async Task AddAsync(Enrollment entity, CancellationToken cancellationToken)
    {
        await dbContext.Enrollments.AddAsync(entity, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Enrollment> entities, CancellationToken cancellationToken)
    {
        await dbContext.Enrollments.AddRangeAsync(entities, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Enrollment>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Enrollments.ToListAsync(cancellationToken);
    }

    public async Task<Enrollment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Enrollments.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(Enrollment entity, CancellationToken cancellationToken)
    {
        dbContext.Enrollments.Update(entity);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Enrollment entity, CancellationToken cancellationToken)
    {
        dbContext.Enrollments.Remove(entity);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task<(List<Enrollment> Enrollments, int TotalCount)> GetPagedEnrollmentsAsync(
        EnrollmentListCriteria criteria,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Enrollments
            .Include(e => e.Schedule!)
            .ThenInclude(s => s.Course)
            .Include(e => e.Schedule!)
            .ThenInclude(s => s.Room)
            .Include(e => e.Schedule!)
            .AsNoTracking();

        // Lọc theo CourseId (qua Schedule)
        if (criteria.CourseId.HasValue && criteria.CourseId.Value != Guid.Empty)
            query = query.Where(e => e.Schedule != null && e.Schedule.CourseId == criteria.CourseId.Value);

        // Lọc theo StudentId
        if (criteria.StudentId.HasValue && criteria.StudentId.Value != Guid.Empty)
            query = query.Where(e => e.StudentId == criteria.StudentId.Value);

        // Lọc theo ScheduleId
        if (criteria.ScheduleId.HasValue && criteria.ScheduleId.Value != Guid.Empty)
            query = query.Where(e => e.ScheduleId == criteria.ScheduleId.Value);

        // Lọc theo Status
        if (criteria.Status != null) query = query.Where(e => e.Status == criteria.Status);

        // Lọc theo SearchTerm
        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            var searchTermLower = criteria.SearchTerm.ToLower();
            query = query.Where(e => e.Schedule != null &&
                                     e.Schedule.Course != null &&
                                     e.Schedule.Course.Name.ToLower().Contains(searchTermLower));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Sắp xếp
        if (!string.IsNullOrEmpty(criteria.SortBy))
            query = criteria.SortBy.ToLower() switch
            {
                "enrollmentdate" => criteria.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(e => e.EnrolledAt)
                    : query.OrderBy(e => e.EnrolledAt),
                "studentid" => criteria.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(e => e.StudentId)
                    : query.OrderBy(e => e.StudentId),
                "coursename" => criteria.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(e => e.Schedule!.Course!.Name)
                    : query.OrderBy(e => e.Schedule!.Course!.Name),
                _ => query.OrderBy(e => e.Id)
            };
        else
            query = query.OrderBy(e => e.EnrolledAt);

        // Phân trang
        var enrollments = await query
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return (enrollments, totalCount);
    }
}
