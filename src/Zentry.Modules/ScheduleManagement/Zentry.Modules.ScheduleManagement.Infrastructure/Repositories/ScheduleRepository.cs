using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Zentry.Modules.ScheduleManagement.Application.Abstractions;
using Zentry.Modules.ScheduleManagement.Application.Features.GetSchedules;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.Modules.ScheduleManagement.Infrastructure.Persistence;
using Zentry.SharedKernel.Constants.Schedule;

namespace Zentry.Modules.ScheduleManagement.Infrastructure.Repositories;

public class ScheduleRepository(ScheduleDbContext dbContext) : IScheduleRepository
{
    public async Task<List<Schedule>> GetLecturerSchedulesForDateAsync(
        Guid lecturerId,
        DateTime date,
        WeekDayEnum weekDay,
        CancellationToken cancellationToken)
    {
        var dateOnly = DateOnly.FromDateTime(date);

        return await dbContext.Schedules
            .Include(s => s.ClassSection!)
            .ThenInclude(cs => cs.Course)
            .Include(s => s.Room)
            .Where(s => s.ClassSection!.LecturerId == lecturerId
                        && s.WeekDay == weekDay
                        && s.StartDate <= dateOnly
                        && s.EndDate >= dateOnly)
            .ToListAsync(cancellationToken);
    }

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

    public async Task UpdateAsync(Schedule entity, CancellationToken cancellationToken)
    {
        dbContext.Schedules.Update(entity);
        await SaveChangesAsync(cancellationToken);
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

    public async Task<bool> IsLecturerAvailableAsync(Guid lecturerId, WeekDayEnum weekDay, TimeOnly startTime,
        TimeOnly endTime, CancellationToken cancellationToken)
    {
        var overlap = await dbContext.Schedules
            .Include(s => s.ClassSection)
            .ThenInclude(cs => cs!.Course)
            .Include(s => s.Room)
            .AnyAsync(s => s.ClassSection!.LecturerId == lecturerId &&
                           s.WeekDay == weekDay &&
                           s.StartTime < endTime &&
                           s.EndTime > startTime,
                cancellationToken);

        return !overlap;
    }

    public async Task<bool> IsRoomAvailableAsync(Guid roomId, WeekDayEnum weekDay, TimeOnly startTime,
        TimeOnly endTime, CancellationToken cancellationToken)
    {
        var overlap = await dbContext.Schedules
            .AnyAsync(s => s.RoomId == roomId &&
                           s.WeekDay == weekDay &&
                           s.StartTime < endTime &&
                           s.EndTime > startTime,
                cancellationToken);

        return !overlap;
    }

    public async Task<Tuple<List<Schedule>, int>> GetPagedSchedulesAsync(ScheduleListCriteria criteria,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Schedules
            .Include(s => s.ClassSection)
            .ThenInclude(cs => cs.Course)
            .AsQueryable();

        if (criteria.LecturerId.HasValue)
            query = query.Where(s => s.ClassSection!.LecturerId == criteria.LecturerId.Value);

        if (criteria.ClassSectionId.HasValue)
            query = query.Where(s => s.ClassSectionId == criteria.ClassSectionId.Value);

        if (criteria.RoomId.HasValue)
            query = query.Where(s => s.RoomId == criteria.RoomId.Value);

        if (criteria.WeekDay != null)
            query = query.Where(s => s.WeekDay.Id == criteria.WeekDay.Id);

        var totalCount = await query.CountAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(criteria.SortBy))
        {
            Expression<Func<Schedule, object>> orderExpr = criteria.SortBy.ToLower() switch
            {
                "coursename" => s => s.ClassSection!.Course!.Name,
                "roomname" => s => s.Room!.RoomName,
                "starttime" => s => s.StartTime,
                "endtime" => s => s.EndTime,
                "weekday" => s => s.WeekDay,
                _ => s => s.StartTime
            };

            query = criteria.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(orderExpr)
                : query.OrderBy(orderExpr);
        }
        else
        {
            query = query.OrderBy(s => s.WeekDay).ThenBy(s => s.StartTime);
        }

        var schedules = await query
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return Tuple.Create(schedules, totalCount);
    }

    public async Task<Tuple<List<Schedule>, int>> GetPagedSchedulesWithIncludesAsync(ScheduleListCriteria criteria,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Schedules
            .Include(s => s.ClassSection)
            .ThenInclude(cs => cs.Course)
            .Include(s => s.Room)
            .AsQueryable();

        if (criteria.LecturerId.HasValue)
            query = query.Where(s => s.ClassSection!.LecturerId == criteria.LecturerId.Value);

        if (criteria.ClassSectionId.HasValue)
            query = query.Where(s => s.ClassSectionId == criteria.ClassSectionId.Value);

        if (criteria.RoomId.HasValue)
            query = query.Where(s => s.RoomId == criteria.RoomId.Value);

        if (criteria.WeekDay != null)
            query = query.Where(s => s.WeekDay.Id == criteria.WeekDay.Id);

        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            var st = criteria.SearchTerm.ToLower();
            query = query.Where(s =>
                s.ClassSection!.Course!.Name.ToLower().Contains(st) ||
                s.ClassSection!.Course!.Code.ToLower().Contains(st) ||
                s.Room!.RoomName.ToLower().Contains(st) ||
                s.Room!.Building.ToLower().Contains(st));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(criteria.SortBy))
            query = criteria.SortBy.ToLower() switch
            {
                "coursename" => criteria.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(s => s.ClassSection!.Course!.Name)
                    : query.OrderBy(s => s.ClassSection!.Course!.Name),
                "roomname" => criteria.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(s => s.Room!.RoomName)
                    : query.OrderBy(s => s.Room!.RoomName),
                "starttime" => criteria.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(s => s.StartTime)
                    : query.OrderBy(s => s.StartTime),
                "endtime" => criteria.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(s => s.EndTime)
                    : query.OrderBy(s => s.EndTime),
                "weekday" => criteria.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(s => s.WeekDay)
                    : query.OrderBy(s => s.WeekDay),
                _ => query.OrderBy(s => s.WeekDay).ThenBy(s => s.StartTime)
            };
        else
            query = query.OrderBy(s => s.WeekDay).ThenBy(s => s.StartTime);

        var schedules = await query
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return Tuple.Create(schedules, totalCount);
    }

    public async Task<Schedule?> GetByIdWithClassSectionAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Schedules
            .Include(s => s.ClassSection)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }
}