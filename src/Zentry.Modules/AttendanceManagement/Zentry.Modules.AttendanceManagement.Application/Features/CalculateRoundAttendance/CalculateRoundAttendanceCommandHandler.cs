using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Zentry.Infrastructure.Caching;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.Modules.AttendanceManagement.Domain.ValueObjects;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Contracts.Device;
using Zentry.SharedKernel.Contracts.Events;
using Zentry.SharedKernel.Contracts.User;
using Zentry.SharedKernel.Enums.User;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.AttendanceManagement.Application.Features.CalculateRoundAttendance;

public class CalculateRoundAttendanceCommandHandler(
    ILogger<CalculateRoundAttendanceCommandHandler> logger,
    IRedisService redisService,
    ISessionRepository sessionRepository,
    IRoundRepository roundRepository,
    IScanLogWhitelistRepository scanLogWhitelistRepository,
    IScanLogRepository scanLogRepository,
    IRoundTrackRepository roundTrackRepository,
    IStudentTrackRepository studentTrackRepository,
    IPublishEndpoint publishEndpoint,
    IMediator mediator)
    : ICommandHandler<CalculateRoundAttendanceCommand, CalculateRoundAttendanceResponse>
{
    public async Task<CalculateRoundAttendanceResponse> Handle(
        CalculateRoundAttendanceCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Starting attendance calculation for Session {SessionId}, Round {RoundId}",
            request.SessionId, request.RoundId);

        try
        {
            // Validate round exists
            var round = await roundRepository.GetByIdAsync(request.RoundId, cancellationToken);
            if (round is null)
            {
                logger.LogWarning("Round {RoundId} not found", request.RoundId);
                return new CalculateRoundAttendanceResponse(false, "Round not found", 0);
            }

            // Calculate and persist attendance
            var attendedDevicesCount = await CalculateAndPersistRoundAttendance(
                request.SessionId,
                request.RoundId,
                cancellationToken);

            // Check if final processing is needed
            await CheckAndTriggerFinalAttendanceProcessing(round, cancellationToken);

            logger.LogInformation(
                "Successfully calculated attendance for Round {RoundId}: {Count} devices attended",
                request.RoundId, attendedDevicesCount);

            return new CalculateRoundAttendanceResponse(
                true,
                "Attendance calculated successfully",
                attendedDevicesCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error calculating attendance for Session {SessionId}, Round {RoundId}",
                request.SessionId, request.RoundId);

            return new CalculateRoundAttendanceResponse(
                false,
                "An error occurred while calculating attendance",
                0);
        }
    }

    private async Task<int> CalculateAndPersistRoundAttendance(
        Guid sessionId,
        Guid roundId,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Starting attendance calculation for Round {RoundId}", roundId);

            // 1. Get whitelist
            var whitelist = await GetSessionWhitelist(sessionId, cancellationToken);

            // 2. Get scan logs for round
            var scanLogs = await GetRoundScanLogs(sessionId, roundId, cancellationToken);

            // 3. Apply BFS multi-hop algorithm
            var attendedDeviceIds = await CalculateAttendance(whitelist, scanLogs);

            // 4. Persist attendance result
            await PersistAttendanceResult(
                await roundRepository.GetByIdAsync(roundId, cancellationToken) ??
                throw new NotFoundException(nameof(CalculateRoundAttendanceCommandHandler), "Round not found!"),
                attendedDeviceIds, cancellationToken);

            logger.LogInformation(
                "Successfully calculated and persisted attendance for Round {RoundId}: {Count} devices attended",
                roundId, attendedDeviceIds.Count);

            return attendedDeviceIds.Count;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calculating attendance for Round {RoundId}", roundId);
            throw;
        }
    }

    // Copy all the private methods from AttendanceProcessorService here
    private async Task<HashSet<string>> GetSessionWhitelist(Guid sessionId, CancellationToken cancellationToken)
    {
        var cacheKey = $"session_whitelist:{sessionId}";
        logger.LogInformation(
            "Attempting to retrieve whitelist for Session {SessionId} from Redis cache (key: {CacheKey}).",
            sessionId, cacheKey);
        try
        {
            var cachedWhitelist = await redisService.GetAsync<List<string>>(cacheKey);

            if (cachedWhitelist != null)
            {
                if (cachedWhitelist.Count > 0)
                {
                    logger.LogInformation(
                        "Whitelist for Session {SessionId} found in Redis cache. Total devices: {Count}.",
                        sessionId, cachedWhitelist.Count);
                    return [..cachedWhitelist];
                }

                logger.LogWarning(
                    "Whitelist for Session {SessionId} found in Redis cache but is empty. Proceeding to DB or returning empty set.",
                    sessionId);
                return [];
            }

            logger.LogInformation(
                "Whitelist for Session {SessionId} not found in Redis cache or deserialization failed. Fetching from database.",
                sessionId);

            var whitelist = await scanLogWhitelistRepository.GetBySessionIdAsync(sessionId, cancellationToken);

            if (whitelist != null && whitelist.WhitelistedDeviceIds.Count != 0)
                return [..whitelist.WhitelistedDeviceIds.Select(id => id.ToString())];

            logger.LogWarning("No whitelist found or whitelist is empty for Session {SessionId}.", sessionId);
            return [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching whitelist for Session {SessionId}.", sessionId);
            throw;
        }
    }

    private async Task<List<ScanLog>> GetRoundScanLogs(Guid sessionId, Guid roundId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Querying scan logs for Round {RoundId} using IScanLogRepository.", roundId);

        try
        {
            var lecturerId = await sessionRepository.GetLecturerIdBySessionId(sessionId, cancellationToken);
            var isLecturerIdExist =
                await scanLogRepository.HasLecturerScanLogInRoundAsync(roundId, lecturerId, cancellationToken);
            if (!isLecturerIdExist)
            {
                var device = await mediator.Send(new GetDeviceByUserIntegrationQuery(lecturerId), cancellationToken);
                var record = ScanLog.Create(
                    Guid.NewGuid(),
                    device.DeviceId,
                    lecturerId,
                    sessionId,
                    roundId,
                    DateTime.UtcNow,
                    []
                );

                await scanLogRepository.AddScanDataAsync(record);
            }

            var scanLogs = await scanLogRepository.GetScanLogsByRoundIdAsync(roundId, cancellationToken);

            if (scanLogs.Count == 0)
            {
                logger.LogWarning("No scan logs found for Round {RoundId}.", roundId);
                return [];
            }

            logger.LogInformation("Found {Count} scan logs for Round {RoundId}.", scanLogs.Count, roundId);
            return scanLogs;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching scan logs for Round {RoundId}.", roundId);
            throw;
        }
    }

    private async Task<List<string>> CalculateAttendance(HashSet<string> whitelist,
        List<ScanLog> scanLogs)
    {
        logger.LogInformation("Applying BFS multi-hop attendance algorithm with whitelist of {Count} devices",
            whitelist.Count);

        logger.LogInformation("Whitelist keys: {WhitelistKeys}", string.Join(", ", whitelist));

        // 1. Filter submissions from registered devices only
        var filteredScanLogs = FilterWhitelistSubmissions(scanLogs, whitelist);

        // 2. Build neighbor records for each device
        var deviceRecords = await BuildDeviceRecords(filteredScanLogs, whitelist);

        // 3. Find lecturer as BFS root
        var lecturerId = FindLecturerRoot(deviceRecords);

        // 4. Apply BFS algorithm
        var attendedDevices = ApplyBfsAlgorithm(deviceRecords, lecturerId, whitelist);

        // 5. Apply fill-in phase
        var finalAttendance = ApplyFillInPhase(deviceRecords, attendedDevices, whitelist);

        return finalAttendance.OrderBy(x => x).ToList();
    }

    private List<ScanLog> FilterWhitelistSubmissions(List<ScanLog> scanLogs, HashSet<string> whitelist)
    {
        var filtered = scanLogs
            .Where(s => whitelist.Contains(s.DeviceId.ToString()))
            .ToList();

        logger.LogInformation("Filtered {Original} submissions to {Filtered} from whitelisted devices",
            scanLogs.Count, filtered.Count);

        foreach (var scanLog in filtered)
        {
            var scannedDeviceIds = scanLog.ScannedDevices.Select(d => d.DeviceId);
            logger.LogInformation("Submission from {SubmitterId}, scanned: {ScannedDevices}",
                scanLog.DeviceId,
                string.Join(", ", scannedDeviceIds));
        }

        return filtered;
    }

    private async Task<List<DeviceRecord>> BuildDeviceRecords(List<ScanLog> scanLogs, HashSet<string> whitelist)
    {
        logger.LogInformation("Building device records with neighbor scan lists");

        var groupedScanLogs = scanLogs
            .GroupBy(s => s.DeviceId.ToString())
            .ToList();

        var records = new List<DeviceRecord>();

        var allUniqueDeviceIdGuids = groupedScanLogs
            .Select(g => Guid.Parse(g.Key))
            .ToList();

        var deviceRolesMap = new Dictionary<Guid, string>();
        if (allUniqueDeviceIdGuids.Count != 0)
        {
            try
            {
                var getDeviceRolesQuery = new GetDeviceRolesByDevicesIntegrationQuery(allUniqueDeviceIdGuids);
                var response = await mediator.Send(getDeviceRolesQuery);
                deviceRolesMap = response.DeviceRolesMap;
                logger.LogInformation("Fetched roles for {Count} devices in batch query.", deviceRolesMap.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error fetching device roles in batch. Individual lookups may fail. Defaulting to 'Unknown' for affected devices.");
            }
        }

        foreach (var g in groupedScanLogs)
        {
            var deviceIdString = g.Key;
            var deviceIdGuid = Guid.Parse(deviceIdString);

            logger.LogDebug("Processing device {DeviceId} for role and scan list", deviceIdString);

            var role = deviceRolesMap.GetValueOrDefault(deviceIdGuid, "Unknown");
            if (role == "Unknown")
            {
                logger.LogWarning(
                    "Role not found for Device {DeviceId} in batch result or initial error. Defaulting to 'Unknown'.",
                    deviceIdString);
            }

            var scanList = g
                .SelectMany(s => s.ScannedDevices)
                .Where(d => whitelist.Contains(d.DeviceId))
                .GroupBy(d => d.DeviceId)
                .Select(gr => new { Id = gr.Key, Rssi = gr.Max(x => x.Rssi) })
                .OrderByDescending(x => x.Rssi)
                .Select(x => x.Id)
                .ToList();

            logger.LogDebug("Device {DeviceId} ({Role}) has {NeighborCount} neighbors after filtering",
                deviceIdString, role, scanList.Count);

            records.Add(new DeviceRecord
            {
                DeviceId = deviceIdString,
                Role = role,
                ScanList = scanList
            });
        }

        logger.LogInformation("Successfully built {RecordCount} device records", records.Count);

        foreach (var record in records)
            logger.LogInformation("Device {DeviceId} ({Role}) → Neighbors: [{Neighbors}]",
                record.DeviceId,
                record.Role,
                record.ScanList.Count != 0 ? string.Join(", ", record.ScanList) : "No neighbors");

        return records;
    }

    private string FindLecturerRoot(List<DeviceRecord> deviceRecords)
    {
        var lecturerRecord = deviceRecords.FirstOrDefault(r =>
            r.Role.Equals(Role.Lecturer.ToString(), StringComparison.OrdinalIgnoreCase));

        if (lecturerRecord == null)
            throw new InvalidOperationException("No lecturer found in device records for BFS root");

        logger.LogInformation("Found lecturer {LecturerId} as BFS root", lecturerRecord.DeviceId);
        return lecturerRecord.DeviceId;
    }

    private HashSet<string> ApplyBfsAlgorithm(List<DeviceRecord> deviceRecords, string lecturerId,
        HashSet<string> whitelist)
    {
        logger.LogInformation("Starting BFS traversal from lecturer {LecturerId}", lecturerId);

        var scanMap = deviceRecords.ToDictionary(r => r.DeviceId, r => r.ScanList);
        var attendance = new HashSet<string> { lecturerId };
        var queue = new Queue<string>();
        queue.Enqueue(lecturerId);

        while (queue.Any())
        {
            var currentDevice = queue.Dequeue();
            var neighbors = scanMap.GetValueOrDefault(currentDevice, new List<string>());

            foreach (var neighbor in neighbors)
                if (whitelist.Contains(neighbor) && attendance.Add(neighbor))
                    queue.Enqueue(neighbor);
        }

        logger.LogInformation("BFS phase completed: {Count} devices reached", attendance.Count);
        return attendance;
    }

    private HashSet<string> ApplyFillInPhase(List<DeviceRecord> deviceRecords, HashSet<string> attendance,
        HashSet<string> whitelist)
    {
        logger.LogInformation("Applying fill-in phase");

        var finalAttendance = new HashSet<string>(attendance);

        foreach (var deviceRecord in deviceRecords)
        {
            var deviceId = deviceRecord.DeviceId;
            var neighbors = deviceRecord.ScanList;

            if (!finalAttendance.Contains(deviceId) &&
                whitelist.Contains(deviceId) &&
                neighbors.Any(n => finalAttendance.Contains(n)))
            {
                finalAttendance.Add(deviceId);
                logger.LogDebug("Fill-in: Added {DeviceId} (has attended neighbors)", deviceId);
            }
        }

        logger.LogInformation("Fill-in phase completed: {Count} total devices attended", finalAttendance.Count);
        return finalAttendance;
    }

    private async Task PersistAttendanceResult(
        Round currentRound,
        List<string> attendedDeviceIds,
        CancellationToken cancellationToken)
    {
        var sessionId = currentRound.SessionId;
        var roundId = currentRound.Id;

        logger.LogInformation("Persisting attendance result for Round {RoundId}: {Count} devices from BFS results.",
            roundId, attendedDeviceIds.Count);

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

        Dictionary<Guid, Guid> deviceToUserMap = new Dictionary<Guid, Guid>();
        if (attendedDeviceGuids.Any())
        {
            var getUserIdsByDevicesQuery = new GetUserIdsByDevicesIntegrationQuery(attendedDeviceGuids);
            var getUserIdsByDevicesResponse = await mediator.Send(getUserIdsByDevicesQuery, cancellationToken);
            deviceToUserMap = getUserIdsByDevicesResponse.UserDeviceMap;
        }

        var mergedStudentsInRoundTrack = new Dictionary<Guid, StudentAttendanceInRound>();

        foreach (var existingStudent in roundTrack.Students)
        {
            mergedStudentsInRoundTrack[existingStudent.StudentId] = existingStudent;
        }

        var studentTracksToUpdate = new List<StudentTrack>();

        foreach (var (deviceId, userId) in deviceToUserMap)
        {
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

    private async Task CheckAndTriggerFinalAttendanceProcessing(
        Round currentRound,
        CancellationToken cancellationToken)
    {
        var sessionId = currentRound.SessionId;
        var currentRoundNumber = currentRound.RoundNumber;

        var totalRounds = await roundRepository.CountRoundsBySessionIdAsync(sessionId, cancellationToken);

        if (currentRoundNumber == totalRounds)
        {
            logger.LogInformation(
                "Round {RoundNumber} is the final round for Session {SessionId}. Publishing final attendance processing message.",
                currentRoundNumber, sessionId);

            var message = new SessionFinalAttendanceToProcess
            {
                SessionId = sessionId
            };
            await publishEndpoint.Publish(message, cancellationToken);

            logger.LogInformation("SessionFinalAttendanceToProcess message published for Session {SessionId}.",
                sessionId);
        }
        else
        {
            logger.LogInformation(
                "Round {RoundNumber} is not the final round ({TotalRounds} total rounds) for Session {SessionId}. No final processing triggered.",
                currentRoundNumber, sessionId, totalRounds);
        }
    }

    // Internal DTO for algorithm processing
    private class DeviceRecord
    {
        public string DeviceId { get; set; }
        public string Role { get; set; }
        public List<string> ScanList { get; set; }
    }
}
