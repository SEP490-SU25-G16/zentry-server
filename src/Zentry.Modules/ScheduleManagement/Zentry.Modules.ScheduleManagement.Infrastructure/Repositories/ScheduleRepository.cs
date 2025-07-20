using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Features.GetSchedules;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.Modules.ScheduleManagement.Domain.Enums;
using Zentry.Modules.ScheduleManagement.Infrastructure.Persistence;

namespace Zentry.Modules.ScheduleManagement.Infrastructure.Repositories;

public class ScheduleRepository(ScheduleDbContext dbContext) : IScheduleRepository
{
    public async Task<IEnumerable<Schedule>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Schedules.ToListAsync(cancellationToken);
    }

    public async Task<Schedule?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Schedules.FindAsync([id], cancellationToken);
    }

    public async Task AddAsync(Schedule entity, CancellationToken cancellationToken)
    {
        await dbContext.Schedules.AddAsync(entity, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Schedule> entities, CancellationToken cancellationToken)
    {
        await dbContext.Schedules.AddRangeAsync(entities, cancellationToken);
    }

    public async Task UpdateAsync(Schedule entity, CancellationToken cancellationToke)
    {
        dbContext.Schedules.Update(entity);
        await SaveChangesAsync(cancellationToke);
    }

    public async Task DeleteAsync(Schedule entity, CancellationToken cancellationToken)
    {
        dbContext.Schedules.Remove(entity);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    // Triển khai IsLecturerAvailableAsync
    public async Task<bool> IsLecturerAvailableAsync(Guid lecturerId, DayOfWeekEnum dayOfWeek, DateTime startTime,
        DateTime endTime, CancellationToken cancellationToken)
    {
        // Kiểm tra xem có lịch nào của giảng viên này trùng thời gian và ngày không
        // Logic trùng lặp: (StartA < EndB) AND (EndA > StartB)
        // Chúng ta cần lấy phần giờ (TimeOfDay) để so sánh, vì ngày có thể khác nhau nhưng cùng một ngày trong tuần.
        // Hướng dẫn: Chỉ lấy phần TimeOfDay cho StartTime và EndTime.
        var newStartTimeOfDay = startTime.TimeOfDay;
        var newEndTimeOfDay = endTime.TimeOfDay;

        var overlappingSchedules = await dbContext.Schedules
            .AnyAsync(s =>
                    s.LecturerId == lecturerId &&
                    s.DayOfWeek == dayOfWeek &&
                    // Kiểm tra chồng chéo thời gian
                    // (s.StartTime < endTime) && (s.EndTime > startTime)
                    // Hoặc chính xác hơn với TimeOfDay
                    s.StartTime.TimeOfDay < newEndTimeOfDay && s.EndTime.TimeOfDay > newStartTimeOfDay,
                cancellationToken);

        return !overlappingSchedules;
    }

    // Triển khai IsRoomAvailableAsync
    public async Task<bool> IsRoomAvailableAsync(Guid roomId, DayOfWeekEnum dayOfWeek, DateTime startTime,
        DateTime endTime, CancellationToken cancellationToken)
    {
        // Kiểm tra xem có lịch nào của phòng này trùng thời gian và ngày không
        var newStartTimeOfDay = startTime.TimeOfDay;
        var newEndTimeOfDay = endTime.TimeOfDay;

        var overlappingSchedules = await dbContext.Schedules
            .AnyAsync(s =>
                    s.RoomId == roomId &&
                    s.DayOfWeek == dayOfWeek &&
                    s.StartTime.TimeOfDay < newEndTimeOfDay && s.EndTime.TimeOfDay > newStartTimeOfDay,
                cancellationToken);

        return !overlappingSchedules;
    }

    public async Task<Tuple<List<Schedule>, int>> GetPagedSchedulesAsync(ScheduleListCriteria criteria,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Schedules.AsQueryable();

        // Filtering
        if (criteria.LecturerId.HasValue) query = query.Where(s => s.LecturerId == criteria.LecturerId.Value);
        if (criteria.CourseId.HasValue) query = query.Where(s => s.CourseId == criteria.CourseId.Value);
        if (criteria.RoomId.HasValue) query = query.Where(s => s.RoomId == criteria.RoomId.Value);
        if (criteria.DayOfWeek != null) query = query.Where(s => s.DayOfWeek.Id == criteria.DayOfWeek.Id);

        // Search Term (this will search in related entities later if needed, but for now just on Schedule's own properties)
        // Note: For searching by LecturerName, CourseName, RoomName, you'd typically need to JOIN or do client-side filtering after loading data.
        // For now, let's assume SearchTerm only applies to Schedule's direct properties if applicable, or we remove it here.
        // Given Schedule only has IDs and times, SearchTerm is less relevant directly on Schedule entity.
        // We'll handle this in the handler by filtering *after* lookups if SearchTerm is meant for names.
        // For now, let's omit direct SearchTerm filtering on the Schedule entity unless there's a specific field.
        // If you want to search names at DB level, you'd need direct DB access to Course/Room/User or views/materialized views.


        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Sorting
        if (!string.IsNullOrWhiteSpace(criteria.SortBy))
        {
            Expression<Func<Schedule, object>> orderByExpression = criteria.SortBy.ToLower() switch
            {
                "starttime" => s => s.StartTime,
                "endtime" => s => s.EndTime,
                "dayofweek" => s => s.DayOfWeek,
                "createdat" => s => s.CreatedAt,
                // Sorting by LecturerName, CourseName, RoomName cannot be done directly here
                // without joining or pre-loading related entities.
                // For simplicity, we'll sort by Schedule's own properties.
                _ => s => s.CreatedAt // Default sort
            };

            query = criteria.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(orderByExpression)
                : query.OrderBy(orderByExpression);
        }
        else
        {
            query = query.OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime); // Default sort
        }

        // Pagination
        var schedules = await query
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return Tuple.Create(schedules, totalCount);
    }

    public async Task<Tuple<List<Schedule>, int>> GetPagedSchedulesWithIncludesAsync(
        ScheduleListCriteria criteria,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Schedules
            .Include(s => s.Course) // Include Course
            .Include(s => s.Room) // Include Room
            .AsQueryable();

        // Apply filtering
        if (criteria.LecturerId.HasValue)
            query = query.Where(s => s.LecturerId == criteria.LecturerId.Value);
        if (criteria.CourseId.HasValue)
            query = query.Where(s => s.CourseId == criteria.CourseId.Value);
        if (criteria.RoomId.HasValue)
            query = query.Where(s => s.RoomId == criteria.RoomId.Value);
        if (criteria.DayOfWeek != null)
            query = query.Where(s => s.DayOfWeek.Id == criteria.DayOfWeek.Id);

        // Search trong related entities
        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            var searchTerm = criteria.SearchTerm.ToLower();
            query = query.Where(s =>
                s.Course!.Name.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase) ||
                s.Course!.Code.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase) ||
                s.Room!.RoomName.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase) ||
                s.Room!.Building.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase)
            );
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Sorting with related entities
        if (!string.IsNullOrWhiteSpace(criteria.SortBy))
            query = criteria.SortBy.ToLower() switch
            {
                "coursename" => criteria.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(s => s.Course!.Name)
                    : query.OrderBy(s => s.Course!.Name),
                "roomname" => criteria.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(s => s.Room!.RoomName)
                    : query.OrderBy(s => s.Room!.RoomName),
                "starttime" => criteria.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(s => s.StartTime)
                    : query.OrderBy(s => s.StartTime),
                "endtime" => criteria.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(s => s.EndTime)
                    : query.OrderBy(s => s.EndTime),
                "dayofweek" => criteria.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(s => s.DayOfWeek)
                    : query.OrderBy(s => s.DayOfWeek),
                _ => query.OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime)
            };
        else
            query = query.OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime);

        // Pagination
        var schedules = await query
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return Tuple.Create(schedules, totalCount);
    }
}
