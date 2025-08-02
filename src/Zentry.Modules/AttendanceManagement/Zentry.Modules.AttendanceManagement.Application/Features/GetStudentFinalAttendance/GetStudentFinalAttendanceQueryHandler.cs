using MediatR;
using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Application.Dtos;
using Zentry.Modules.AttendanceManagement.Domain.ValueObjects;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.User;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.AttendanceManagement.Application.Features.GetStudentFinalAttendance;

public class GetStudentFinalAttendanceQueryHandler(
    ISessionRepository sessionRepository,
    IRoundRepository roundRepository,
    IStudentTrackRepository studentTrackRepository,
    IMediator mediator,
    ILogger<GetStudentFinalAttendanceQueryHandler> logger)
    : IQueryHandler<GetStudentFinalAttendanceQuery, StudentFinalAttendanceDto>
{
    public async Task<StudentFinalAttendanceDto> Handle(GetStudentFinalAttendanceQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Handling GetStudentFinalAttendanceQuery for SessionId: {SessionId}, StudentId: {StudentId}",
            request.SessionId, request.StudentId);

        // 1. Lấy thông tin Session và tổng số rounds
        var session = await sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
        if (session is null)
        {
            throw new NotFoundException("Session", $"Phiên học với ID '{request.SessionId}' không tìm thấy.");
        }

        var allRounds = await roundRepository.GetRoundsBySessionIdAsync(request.SessionId, cancellationToken);
        var totalRounds = allRounds.Count;
        if (totalRounds == 0)
        {
            logger.LogWarning("No rounds found for SessionId: {SessionId}.", request.SessionId);
            return new StudentFinalAttendanceDto
            {
                StudentId = request.StudentId,
                SessionId = request.SessionId,
                FinalAttendancePercentage = 0,
                TotalRounds = 0,
                AttendedRoundsCount = 0,
                MissedRoundsCount = 0
            };
        }

        // 2. Lấy thông tin sinh viên từ UserManagement module
        var userResponse = await mediator.Send(new GetUsersByIdsIntegrationQuery(new List<Guid> { request.StudentId }),
            cancellationToken);
        var studentInfo = userResponse.Users.FirstOrDefault();
        if (studentInfo == null)
        {
            logger.LogWarning("Student with ID {StudentId} not found.", request.StudentId);
            throw new NotFoundException("Student", $"Sinh viên với ID '{request.StudentId}' không tìm thấy.");
        }

        // 3. Lấy kết quả điểm danh của sinh viên từ Marten (StudentTrack)
        var studentTrack = await studentTrackRepository.GetByIdAsync(request.StudentId, cancellationToken);

        // 4. Kết hợp dữ liệu và tính toán
        var attendedRounds = studentTrack?.Rounds.ToDictionary(r => r.RoundId) ??
                             new Dictionary<Guid, RoundParticipation>();
        var roundDetails = new List<RoundAttendanceDetailDto>();
        var attendedRoundsCount = 0;

        foreach (var round in allRounds)
        {
            var isAttended = attendedRounds.TryGetValue(round.Id, out var participation) && participation.IsAttended;
            if (isAttended)
            {
                attendedRoundsCount++;
            }

            roundDetails.Add(new RoundAttendanceDetailDto
            {
                RoundId = round.Id,
                RoundNumber = round.RoundNumber,
                IsAttended = isAttended,
                AttendedTime = participation?.AttendedTime
            });
        }

        var finalPercentage = totalRounds > 0 ? (double)attendedRoundsCount / totalRounds * 100 : 0;

        // 5. Ánh xạ và trả về DTO
        return new StudentFinalAttendanceDto
        {
            StudentId = request.StudentId,
            FullName = studentInfo.FullName,
            SessionId = request.SessionId,
            FinalAttendancePercentage = finalPercentage,
            TotalRounds = totalRounds,
            AttendedRoundsCount = attendedRoundsCount,
            MissedRoundsCount = totalRounds - attendedRoundsCount,
            RoundDetails = roundDetails
        };
    }
}
