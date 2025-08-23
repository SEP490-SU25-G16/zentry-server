using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Features.Courses.GetCourses;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.Modules.ScheduleManagement.Infrastructure.Persistence;

namespace Zentry.Modules.ScheduleManagement.Infrastructure.Repositories;

public class CourseRepository(ScheduleDbContext dbContext) : ICourseRepository
{
    public async Task<int> CountTotalCoursesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Courses.Where(c => !c.IsDeleted).CountAsync(cancellationToken);
    }

    public async Task AddAsync(Course entity, CancellationToken cancellationToken)
    {
        await dbContext.Courses.AddAsync(entity, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Course> entities, CancellationToken cancellationToken)
    {
        await dbContext.Courses.AddRangeAsync(entities, cancellationToken);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var course = await dbContext.Courses.FindAsync([id], cancellationToken);

        if (course is not null)
        {
            course.Delete();
            dbContext.Courses.Update(course);
            await SaveChangesAsync(cancellationToken);
        }
    }


    public async Task<IEnumerable<Course>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Courses.ToListAsync(cancellationToken);
    }

    public async Task<Course?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Courses
            .Where(c => !c.IsDeleted)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> IsCodeUniqueAsync(string code, CancellationToken cancellationToken)
    {
        // Quan trọng: Kiểm tra tính duy nhất cần bao gồm cả các bản ghi đã bị xóa mềm
        // để tránh tạo một khóa học mới với cùng code của khóa học đã xóa mềm.
        // Hoặc bạn có thể bỏ qua bản ghi đã xóa mềm nếu logic nghiệp vụ cho phép tái sử dụng code.
        // Ở đây, tôi sẽ kiểm tra DUY NHẤT TRONG SỐ CÁC KHÓA HỌC HIỆN CÓ (chưa xóa mềm)
        // Nếu bạn muốn kiểm tra trên tất cả các bản ghi (kể cả đã xóa mềm), bạn cần dùng IgnoreQueryFilters()
        return !await dbContext.Courses
            .IgnoreQueryFilters() // Tạm thời bỏ qua global filter để kiểm tra tất cả các bản ghi
            .AnyAsync(c => c.Code == code, cancellationToken);
    }


    public async Task DeleteAsync(Course entity, CancellationToken cancellationToken)
    {
        dbContext.Courses.Remove(entity);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Course entity, CancellationToken cancellationToken)
    {
        dbContext.Courses.Update(entity);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task<Tuple<List<Course>, int>> GetPagedCoursesAsync(CourseListCriteria criteria,
        CancellationToken cancellationToken)
    {
        // Query filter đã tự động được áp dụng tại đây
        var query = dbContext.Courses.AsQueryable();

        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
            query = query.Where(c =>
                c.Name.Contains(criteria.SearchTerm) ||
                c.Code.Contains(criteria.SearchTerm) ||
                (c.Description != null && c.Description.Contains(criteria.SearchTerm))
            );

        var totalCount = await query.CountAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(criteria.SortBy))
        {
            Expression<Func<Course, object>> orderByExpression = criteria.SortBy.ToLower() switch
            {
                "code" => c => c.Code,
                "name" => c => c.Name,
                "createdat" => c => c.CreatedAt,
                _ => c => c.CreatedAt
            };

            query = criteria.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(orderByExpression)
                : query.OrderBy(orderByExpression);
        }
        else
        {
            query = query.OrderByDescending(c => c.CreatedAt);
        }

        var courses = await query
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return Tuple.Create(courses, totalCount);
    }
}
