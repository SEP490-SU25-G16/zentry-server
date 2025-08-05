using MediatR;
using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Device;
using Zentry.SharedKernel.Contracts.User;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.AttendanceManagement.Application.Features.GetRoundResult;

public class GetRoundResultQueryHandler(
    IRoundRepository roundRepository,
    IRoundTrackRepository roundTrackRepository,
    ISessionWhitelistRepository sessionWhitelistRepository,
    ISessionRepository sessionRepository,
    IMediator mediator,
    ILogger<GetRoundResultQueryHandler> logger)
    : IQueryHandler<GetRoundResultQuery, RoundResultDto>
{
    public async Task<RoundResultDto> Handle(GetRoundResultQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling GetRoundResultQuery for RoundId: {RoundId}", request.RoundId);

        // 1. Lấy thông tin Round để có SessionId
        var round = await roundRepository.GetByIdAsync(request.RoundId, cancellationToken);
        if (round is null)
        {
            logger.LogWarning("Round with ID {RoundId} not found.", request.RoundId);
            throw new NotFoundException("Round", $"Vòng điểm danh với ID '{request.RoundId}' không tìm thấy.");
        }

        var lectureId = await sessionRepository.GetLecturerIdBySessionId(round.SessionId, cancellationToken);
        // 2. Lấy danh sách whitelistedDeviceIds từ SessionWhitelist
        var sessionWhitelist = await sessionWhitelistRepository.GetBySessionIdAsync(round.SessionId, cancellationToken);
        if (sessionWhitelist == null || sessionWhitelist.WhitelistedDeviceIds.Count == 0)
        {
            logger.LogWarning("No whitelist found for SessionId: {SessionId}. Returning empty result.",
                round.SessionId);
            return new RoundResultDto
            {
                RoundId = round.Id,
                RoundNumber = round.RoundNumber,
                Status = round.Status.ToString(),
                StartTime = round.StartTime,
                EndTime = round.EndTime,
                StudentsAttendance = []
            };
        }

        var deviceIds = sessionWhitelist.WhitelistedDeviceIds;

        // 3. Lấy danh sách UserIds từ DeviceIds (qua Device module)
        var userIdsByDevicesResponse = await mediator.Send(
            new GetUserIdsByDevicesIntegrationQuery(deviceIds),
            cancellationToken);

        var userIds = userIdsByDevicesResponse.UserDeviceMap.Values
            .Where(id => !Equals(id, lectureId))
            .ToList();

        if (userIds.Count == 0)
        {
            logger.LogInformation("No user IDs found for the whitelisted devices. Returning empty result.");
            return new RoundResultDto
            {
                RoundId = round.Id,
                RoundNumber = round.RoundNumber,
                Status = round.Status.ToString(),
                StartTime = round.StartTime,
                EndTime = round.EndTime,
                StudentsAttendance = []
            };
        }

        // 4. Lấy thông tin cơ bản của User từ UserManagement module
        var usersResponse = await mediator.Send(
            new GetUsersByIdsIntegrationQuery(userIds),
            cancellationToken);

        var allStudents = usersResponse.Users;

        // 5. Lấy kết quả điểm danh của round từ Marten (RoundTrack)
        var roundTrack = await roundTrackRepository.GetByIdAsync(request.RoundId, cancellationToken);

        var attendedStudentsMap = new Dictionary<Guid, (bool IsAttended, DateTime? AttendedTime)>();
        if (roundTrack != null && roundTrack.Students.Count != 0)
            attendedStudentsMap = roundTrack.Students.ToDictionary(
                s => s.StudentId,
                s => (s.IsAttended, s.AttendedTime)
            );

        // 6. Kết hợp dữ liệu và tạo DTO trả về
        var studentsAttendance = allStudents.Select(student =>
        {
            var (isAttended, attendedTime) = attendedStudentsMap.GetValueOrDefault(student.Id, (false, null));

            return new StudentAttendanceDto
            {
                StudentId = student.Id,
                FullName = student.FullName,
                IsAttended = isAttended,
                AttendedTime = attendedTime
            };
        }).ToList();

        // 7. Ánh xạ và trả về DTO cuối cùng
        return new RoundResultDto
        {
            RoundId = round.Id,
            RoundNumber = round.RoundNumber,
            Status = round.Status.ToString(),
            StartTime = round.StartTime,
            EndTime = round.EndTime,
            StudentsAttendance = studentsAttendance
        };
    }
}