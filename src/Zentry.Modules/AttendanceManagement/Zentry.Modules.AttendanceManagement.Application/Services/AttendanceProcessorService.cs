using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Zentry.Infrastructure.Caching;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Application.Services.Interface;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.Modules.AttendanceManagement.Domain.ValueObjects;
using Zentry.SharedKernel.Contracts.Device;
using Zentry.SharedKernel.Contracts.Events;
using Zentry.SharedKernel.Contracts.User;
using Zentry.SharedKernel.Enums.Attendance;
using Zentry.SharedKernel.Enums.User;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.AttendanceManagement.Application.Services;

public class AttendanceProcessorService(
    ILogger<AttendanceProcessorService> logger,
    IRoundRepository roundRepository,
    IScanLogWhitelistRepository scanLogWhitelistRepository,
    IScanLogRepository scanLogRepository,
    IRedisService redisService,
    IMediator mediator,
    IRoundTrackRepository roundTrackRepository,
    IStudentTrackRepository studentTrackRepository,
    IPublishEndpoint publishEndpoint)
    : IAttendanceProcessorService
{
    public async Task ProcessBluetoothScanData(ProcessScanDataMessage message, CancellationToken cancellationToken)
    {
        var currentRoundId = message.RoundId;
        Round? currentRound = null;
        if (currentRoundId != Guid.Empty)
        {
            currentRound = await roundRepository.GetByIdAsync(currentRoundId, cancellationToken);
            if (currentRound is null)
            {
                logger.LogWarning(
                    "ProcessBluetoothScanData: Round {RoundId} (from event) not found for Session {SessionId}. Scan data logged but no round-specific processing.",
                    currentRoundId, message.SessionId);
                return;
            }
        }
        else
        {
            logger.LogWarning(
                "ProcessBluetoothScanData: Event for Session {SessionId} received with empty RoundId. Scan data logged but no round-specific processing.",
                message.SessionId);
            return;
        }

        logger.LogInformation(
            "Scan data for Session {SessionId} assigned to Round {RoundId} (RoundNumber: {RoundNumber}).",
            message.SessionId, currentRound.Id, currentRound.RoundNumber);

        if (Equals(currentRound.Status, RoundStatus.Pending))
        {
            currentRound.UpdateStatus(RoundStatus.Active);
            await roundRepository.UpdateAsync(currentRound, cancellationToken);
            logger.LogInformation("Updated Round {RoundId} status to Active.", currentRound.Id);
        }

        await CalculateAndPersistRoundAttendance(
            message.SessionId,
            currentRoundId,
            cancellationToken);
        await CheckAndTriggerFinalAttendanceProcessing(currentRound, cancellationToken);
        logger.LogInformation("Finished processing scan data for SessionId: {SessionId}.", message.SessionId);
    }


    private async Task CalculateAndPersistRoundAttendance(
        Guid sessionId,
        Guid roundId,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Starting attendance calculation for Round {RoundId}", roundId);

            // 1. Lấy whitelist (danh sách thiết bị đã đăng ký)
            var whitelist = await GetSessionWhitelist(sessionId, cancellationToken);

            // 2. Truy vấn scan logs cho round
            var scanLogs = await GetRoundScanLogs(roundId, cancellationToken);

            // 3. Áp dụng thuật toán BFS multi-hop
            var attendedDeviceIds = await CalculateAttendance(whitelist, scanLogs);

            // 4. Lưu kết quả điểm danh
            await PersistAttendanceResult(
                await roundRepository.GetByIdAsync(roundId, cancellationToken) ??
                throw new NotFoundException(nameof(AttendanceProcessorService), "Round not found!"),
                attendedDeviceIds, cancellationToken);

            logger.LogInformation(
                "Successfully calculated and persisted attendance for Round {RoundId}: {Count} devices attended",
                roundId, attendedDeviceIds.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calculating attendance for Round {RoundId}", roundId);
            throw;
        }
    }

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

    private async Task<List<ScanLog>> GetRoundScanLogs(Guid roundId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Querying scan logs for Round {RoundId} using IScanLogRepository.", roundId);

        try
        {
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

    private async Task<List<string>> CalculateAttendance(HashSet<string> whitelist, List<ScanLog> scanLogs)
    {
        logger.LogInformation("Applying BFS multi-hop attendance algorithm with whitelist of {Count} devices",
            whitelist.Count);

        logger.LogInformation("Whitelist keys: {WhitelistKeys}", string.Join(", ", whitelist));

        // 1. Filter submissions từ registered devices only
        var filteredScanLogs = FilterWhitelistSubmissions(scanLogs, whitelist);

        // 2. Build neighbor records cho mỗi device
        var deviceRecords = await BuildDeviceRecords(filteredScanLogs, whitelist);

        // 3. Find lecturer làm BFS root
        var lecturerId = FindLecturerRoot(deviceRecords);

        // 4. Apply BFS algorithm (với whitelist parameter)
        var attendedDevices = ApplyBfsAlgorithm(deviceRecords, lecturerId, whitelist);

        // 5. Apply fill-in phase
        var finalAttendance = ApplyFillInPhase(deviceRecords, attendedDevices, whitelist);

        return finalAttendance.OrderBy(x => x).ToList();
    }

    /// <summary>
    ///     Lọc các ScanLog chỉ từ những thiết bị đã được đăng ký (trong whitelist), về cơ bản thì khi mà phát ra
    ///     thì chỉ những thằng cùng room, cùng session mới nhận ra nhau thôi. Nên bước cần là một thức cẩn thận hơn
    /// </summary>
    /// <param name="scanLogs">Danh sách tất cả ScanLog trong round</param>
    /// <param name="whitelist">HashSet chứa các DeviceId đã đăng ký trong session</param>
    /// <returns>Danh sách ScanLog đã được lọc từ registered devices</returns>
    private List<ScanLog> FilterWhitelistSubmissions(List<ScanLog> scanLogs, HashSet<string> whitelist)
    {
        // Chỉ giữ lại những ScanLog được gửi bởi thiết bị có trong whitelist
        // Convert DeviceId từ Guid sang string để so sánh với whitelist
        var filtered = scanLogs
            .Where(s => whitelist.Contains(s.DeviceId.ToString()))
            .ToList();

        // Log thống kê: có bao nhiêu submission được giữ lại sau khi filter (trong TH này thì bằng nhau)
        logger.LogInformation("Filtered {Original} submissions to {Filtered} from whitelisted devices",
            scanLogs.Count, filtered.Count);

        // Log chi tiết: mỗi submission còn lại được gửi từ device nào và scan được những device nào
        foreach (var scanLog in filtered)
        {
            var scannedDeviceIds = scanLog.ScannedDevices.Select(d => d.DeviceId);
            logger.LogInformation("Submission from {SubmitterId}, scanned: {ScannedDevices}",
                scanLog.DeviceId,
                string.Join(", ", scannedDeviceIds));
        }

        return filtered;
    }

    /// <summary>
    ///     Xây dựng DeviceRecord cho từng thiết bị, chứa danh sách neighbors đã được tối ưu hóa cho thuật toán BFS
    /// </summary>
    /// <param name="scanLogs">Danh sách ScanLog đã được filter (chỉ từ registered devices)</param>
    /// <param name="whitelist">HashSet chứa các DeviceId đã đăng ký để filter neighbors</param>
    /// <returns>Danh sách DeviceRecord với adjacency list tối ưu cho BFS</returns>
    private async Task<List<DeviceRecord>> BuildDeviceRecords(List<ScanLog> scanLogs, HashSet<string> whitelist)
    {
        logger.LogInformation("Building device records with neighbor scan lists");

        var groupedScanLogs = scanLogs
            .GroupBy(s => s.DeviceId.ToString())
            .ToList();

        var records = new List<DeviceRecord>();

        // THAY ĐỔI Ở ĐÂY: Thu thập tất cả DeviceId duy nhất để truy vấn vai trò theo batch
        var allUniqueDeviceIdGuids = groupedScanLogs
            .Select(g => Guid.Parse(g.Key)) // Chắc chắn là Guid
            .ToList();

        Dictionary<Guid, string> deviceRolesMap = new Dictionary<Guid, string>();
        if (allUniqueDeviceIdGuids.Any())
        {
            try
            {
                // Gọi một truy vấn tích hợp mới để lấy vai trò của nhiều thiết bị cùng lúc
                var getDeviceRolesQuery = new GetDeviceRolesByDevicesIntegrationQuery(allUniqueDeviceIdGuids);
                var response = await mediator.Send(getDeviceRolesQuery);
                deviceRolesMap = response.DeviceRolesMap; // Giả định response có một map
                logger.LogInformation("Fetched roles for {Count} devices in batch query.", deviceRolesMap.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error fetching device roles in batch. Individual lookups may fail. Defaulting to 'Unknown' for affected devices.");
                // Trong trường hợp lỗi batch, bạn có thể cân nhắc một chiến lược dự phòng
                // Ví dụ: cố gắng lấy từng cái một hoặc mặc định là "Unknown"
            }
        }

        foreach (var g in groupedScanLogs)
        {
            var deviceIdString = g.Key;
            var deviceIdGuid = Guid.Parse(deviceIdString);

            logger.LogDebug("Processing device {DeviceId} for role and scan list", deviceIdString);

            // Lấy vai trò từ map đã fetch
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
                record.ScanList.Any() ? string.Join(", ", record.ScanList) : "No neighbors");

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

    /// <summary>
    ///     Fill-In Phase: Bổ sung những thiết bị bị "miss" bởi BFS nhưng thực tế đã có mặt
    ///     Ý nghĩa: BFS có thể miss một số thiết bị do:
    ///     - Network topology không hoàn hảo (A thấy B, nhưng B không thấy A)
    ///     - Signal interference tạm thời
    ///     - Timing issues (scan không đồng bộ)
    ///     Fill-in logic: Nếu device X không được BFS detect, nhưng X có scan được
    ///     những device đã attended → X cũng có khả năng cao đã có mặt
    /// </summary>
    /// <param name="deviceRecords">Danh sách device records với neighbor lists</param>
    /// <param name="attendance">Kết quả attendance từ BFS phase</param>
    /// <param name="whitelist">Whitelist để validate devices</param>
    /// <returns>Final attendance sau khi bổ sung fill-in devices</returns>
    private HashSet<string> ApplyFillInPhase(List<DeviceRecord> deviceRecords, HashSet<string> attendance,
        HashSet<string> whitelist)
    {
        logger.LogInformation("Applying fill-in phase");

        // Copy attendance từ BFS làm starting point
        var finalAttendance = new HashSet<string>(attendance);

        // Duyệt qua tất cả devices đã có scan data
        foreach (var deviceRecord in deviceRecords)
        {
            var deviceId = deviceRecord.DeviceId;
            var neighbors = deviceRecord.ScanList; // Danh sách devices mà deviceId này scan được

            // FILL-IN LOGIC: Thêm device nếu thỏa mãn 3 điều kiện
            if (!finalAttendance.Contains(deviceId) && // 1. Chưa được mark attended bởi BFS
                whitelist.Contains(deviceId) && // 2. Là registered device (security check)
                neighbors.Any(n => finalAttendance.Contains(n))) // 3. Có scan được ít nhất 1 device đã attended
            {
                finalAttendance.Add(deviceId);
                logger.LogDebug("Fill-in: Added {DeviceId} (has attended neighbors)", deviceId);
            }
        }

        logger.LogInformation("Fill-in phase completed: {Count} total devices attended", finalAttendance.Count);
        return finalAttendance;
    }

    /// <summary>
    ///     Điểm danh dựa trên các device đã được phát hiện
    /// </summary>
    /// <param name="currentRound">Đối tượng Round hiện tại</param>
    /// <param name="attendedDeviceIds">Danh sách DeviceId (string format) đã được BFS algorithm phát hiện có mặt</param>
    /// <param name="cancellationToken">Token để cancel operation</param>
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

        if (!attendedDeviceGuids.Any())
        {
            logger.LogWarning(
                "No valid attended device GUIDs to process for Round {RoundId}. Skipping persistence of attended users.",
                roundId);
            // Vẫn có thể tạo RoundTrack với danh sách sinh viên rỗng hoặc chỉ có giảng viên nếu cần.
            // Để logic tiếp tục, usersAttendedByDevice sẽ là danh sách rỗng.
            await CreateEmptyRoundTrackAndSave(currentRound, cancellationToken);
            return;
        }

        // BƯỚC 1: Lấy ánh xạ UserId từ DeviceId (sử dụng Integration Query từ DeviceManagement)
        var getUserIdsByDevicesQuery = new GetUserIdsByDevicesIntegrationQuery(attendedDeviceGuids);
        var getUserIdsByDevicesResponse = await mediator.Send(getUserIdsByDevicesQuery, cancellationToken);
        var deviceToUserMap = getUserIdsByDevicesResponse.UserDeviceMap; // Đây là Dictionary<DeviceId, UserId>

        var userIdsToFetchInfo = deviceToUserMap.Values.ToList();

        if (!userIdsToFetchInfo.Any())
        {
            logger.LogWarning(
                "No UserIds found for the detected devices in Round {RoundId}. RoundTrack will contain no student/lecturer attendance.",
                roundId);
            await CreateEmptyRoundTrackAndSave(currentRound, cancellationToken);
            return;
        }

        var studentsInRoundTrack = new List<StudentAttendanceInRound>();
        var studentTracksToUpdate = new List<StudentTrack>();

        foreach (var (deviceId, userId) in deviceToUserMap)
        {
            // Mặc định là đã có mặt vì deviceId này nằm trong danh sách attendedDeviceGuids
            const bool isAttended = true;
            var attendedTime = DateTime.UtcNow;
            var usedDeviceIdString = deviceId.ToString();

            // Sử dụng UserId làm StudentId trong StudentAttendanceInRound và StudentTrack

            studentsInRoundTrack.Add(new StudentAttendanceInRound
            {
                StudentId = userId,
                DeviceId = usedDeviceIdString,
                IsAttended = isAttended,
                AttendedTime = attendedTime
            });

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

        var roundTrack = new RoundTrack(currentRound.Id, currentRound.SessionId, currentRound.RoundNumber,
            currentRound.StartTime)
        {
            ProcessedAt = DateTime.Now,
            Students = studentsInRoundTrack // Chỉ chứa những người được điểm danh qua Bluetooth
        };
        await roundTrackRepository.AddOrUpdateAsync(roundTrack, cancellationToken);
        logger.LogInformation("RoundTrack for Round {RoundId} saved with {AttendedCount} detected students.", roundId,
            studentsInRoundTrack.Count);

        // Lưu tất cả StudentTrack đã cập nhật
        foreach (var st in studentTracksToUpdate)
        {
            await studentTrackRepository.AddOrUpdateAsync(st, cancellationToken);
            logger.LogDebug("StudentTrack for Student {StudentId} updated.", st.Id);
        }

        logger.LogInformation("Finished persisting attendance results for Round {RoundId}.", roundId);
    }

    // Hàm phụ trợ để tạo RoundTrack rỗng nếu không có thiết bị nào được phát hiện
    private async Task CreateEmptyRoundTrackAndSave(Round currentRound, CancellationToken cancellationToken)
    {
        var roundTrack = new RoundTrack(currentRound.Id, currentRound.SessionId, currentRound.RoundNumber,
            currentRound.StartTime)
        {
            ProcessedAt = DateTime.Now,
            Students = []
        };
        await roundTrackRepository.AddOrUpdateAsync(roundTrack, cancellationToken);
        logger.LogInformation("Empty RoundTrack for Round {RoundId} saved.", currentRound.Id);
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
