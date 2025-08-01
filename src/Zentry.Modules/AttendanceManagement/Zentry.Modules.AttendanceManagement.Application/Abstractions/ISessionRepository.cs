﻿using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Abstractions.Data;
using Zentry.SharedKernel.Constants.Attendance;

namespace Zentry.Modules.AttendanceManagement.Application.Abstractions;

public interface ISessionRepository : IRepository<Session, Guid>
{
    Task<IEnumerable<Session>> GetSessionsByScheduleIdAndStatusAsync(Guid scheduleId, SessionStatus status,
        CancellationToken cancellationToken);

    Task<Session?> GetActiveSessionByScheduleId(Guid scheduleId, CancellationToken cancellationToken);
    Task<List<Session>> GetSessionsByScheduleIdAsync(Guid scheduleId, CancellationToken cancellationToken);

    public Task<Session?> GetSessionByScheduleIdAndDate(Guid scheduleId, DateTime date,
        CancellationToken cancellationToken);

    public Task<Guid> GetLecturerIdBySessionId(Guid sessionId, CancellationToken cancellationToken);
}