using MediatR;
using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Application.Services.Interface;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.Modules.AttendanceManagement.Domain.ValueObjects;
using Zentry.SharedKernel.Contracts.Device;

namespace Zentry.Modules.AttendanceManagement.Application.Services;

public class AttendancePersistenceService(
    IRoundTrackRepository roundTrackRepository,
    IStudentTrackRepository studentTrackRepository,
    ISessionRepository sessionRepository,
    IMediator mediator,
    ILogger<AttendancePersistenceService> logger) : IAttendancePersistenceService
{
    public async Task PersistAttendanceResult(
        Round currentRound,
        List<string> attendedDeviceIds,
        CancellationToken cancellationToken)
    {
        var sessionId = currentRound.SessionId;
        var roundId = currentRound.Id;

        logger.LogInformation("Persisting attendance result for Round {RoundId}: {Count} devices from BFS results.",
            roundId, attendedDeviceIds.Count);

        var lecturerId = await sessionRepository.GetLecturerIdBySessionId(sessionId, cancellationToken);

        var attendedDeviceGuids = attendedDeviceIds
            .Where(id => Guid.TryParse(id, out _))
            .Select(Guid.Parse)
            .ToList();

        var roundTrack = await roundTrackRepository.GetByIdAsync(roundId, cancellationToken);
        if (roundTrack == null)
        {
            roundTrack = new RoundTrack(currentRound.Id, currentRound.SessionId, currentRound.RoundNumber,
                currentRound.StartTime);
            logger.LogInformation("Creating new RoundTrack for Round {RoundId}.", roundId);
        }
        else
        {
            logger.LogInformation("Updating existing RoundTrack for Round {RoundId}.", roundId);
        }

        roundTrack.ProcessedAt = DateTime.Now;

        var deviceToUserMap = new Dictionary<Guid, Guid>();
        if (attendedDeviceGuids.Count != 0)
        {
            var getUserIdsByDevicesQuery = new GetUserIdsByDevicesIntegrationQuery(attendedDeviceGuids);
            var getUserIdsByDevicesResponse = await mediator.Send(getUserIdsByDevicesQuery, cancellationToken);
            deviceToUserMap = getUserIdsByDevicesResponse.UserDeviceMap;
        }

        var mergedStudentsInRoundTrack = new Dictionary<Guid, StudentAttendanceInRound>();

        foreach (var existingStudent in roundTrack.Students)
            mergedStudentsInRoundTrack[existingStudent.StudentId] = existingStudent;


        var studentTracksToUpdate = new List<StudentTrack>();

        foreach (var (deviceId, userId) in deviceToUserMap)
        {
            if (userId == lecturerId) continue;
            const bool isAttended = true;
            var attendedTime = DateTime.UtcNow;
            var usedDeviceIdString = deviceId.ToString();

            if (mergedStudentsInRoundTrack.TryGetValue(userId, out var existingStudentInMerged))
            {
                existingStudentInMerged.DeviceId = usedDeviceIdString;
                existingStudentInMerged.IsAttended = isAttended;
                existingStudentInMerged.AttendedTime = attendedTime;
            }
            else
            {
                mergedStudentsInRoundTrack.Add(userId, new StudentAttendanceInRound
                {
                    StudentId = userId,
                    DeviceId = usedDeviceIdString,
                    IsAttended = isAttended,
                    AttendedTime = attendedTime
                });
            }

            var studentTrack = await studentTrackRepository.GetByIdAsync(userId, cancellationToken);
            if (studentTrack == null)
                studentTrack = new StudentTrack(sessionId, userId, usedDeviceIdString);
            else
                studentTrack.DeviceId = usedDeviceIdString;

            var existingRoundParticipation = studentTrack.Rounds.FirstOrDefault(rp => rp.RoundId == roundId);
            if (existingRoundParticipation != null)
            {
                existingRoundParticipation.IsAttended = isAttended;
                existingRoundParticipation.AttendedTime = attendedTime;
                existingRoundParticipation.RoundNumber = currentRound.RoundNumber;
            }
            else
            {
                studentTrack.Rounds.Add(new RoundParticipation
                {
                    RoundId = roundId,
                    SessionId = currentRound.SessionId,
                    RoundNumber = currentRound.RoundNumber,
                    IsAttended = isAttended,
                    AttendedTime = attendedTime
                });
            }

            studentTracksToUpdate.Add(studentTrack);
        }

        roundTrack.Students = mergedStudentsInRoundTrack.Values.ToList();

        await roundTrackRepository.AddOrUpdateAsync(roundTrack, cancellationToken);
        logger.LogInformation("RoundTrack for Round {RoundId} saved with {AttendedCount} detected students.", roundId,
            roundTrack.Students.Count);

        foreach (var st in studentTracksToUpdate)
        {
            await studentTrackRepository.AddOrUpdateAsync(st, cancellationToken);
            logger.LogDebug("StudentTrack for Student {StudentId} updated.", st.Id);
        }

        logger.LogInformation("Finished persisting attendance results for Round {RoundId}.", roundId);
    }
}