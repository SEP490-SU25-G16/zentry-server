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
        // Kiểm tra xem đã có bản ghi ghi danh active nào cho cặp StudentId và ScheduleId này chưa
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

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    // --- Các phương thức khác của IRepository<Enrollment, Guid> ---
    public async Task<IEnumerable<Enrollment>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Enrollments.ToListAsync(cancellationToken);
    }

    public async Task<Enrollment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Enrollments.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public void Update(Enrollment entity)
    {
        dbContext.Enrollments.Update(entity);
    }

    public void Delete(Enrollment entity)
    {
        dbContext.Enrollments.Remove(entity);
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
            .ThenInclude(s => s.Lecturer)
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
        if (criteria.Status.HasValue) query = query.Where(e => e.Status == criteria.Status.Value);

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
            switch (criteria.SortBy.ToLower())
            {
                case "enrollmentdate":
                    query = criteria.SortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(e => e.EnrolledAt)
                        : query.OrderBy(e => e.EnrolledAt);
                    break;
                case "studentid":
                    query = criteria.SortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(e => e.StudentId)
                        : query.OrderBy(e => e.StudentId);
                    break;
                case "coursename":
                    query = criteria.SortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(e => e.Schedule!.Course!.Name)
                        : query.OrderBy(e => e.Schedule!.Course!.Name);
                    break;
                default:
                    query = query.OrderBy(e => e.Id);
                    break;
            }
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