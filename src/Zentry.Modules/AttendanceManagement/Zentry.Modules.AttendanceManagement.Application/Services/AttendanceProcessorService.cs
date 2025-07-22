using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Application.Services.Interface;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Contracts.Events;

namespace Zentry.Modules.AttendanceManagement.Application.Services;

public class AttendanceProcessorService(
    ILogger<AttendanceProcessorService> logger,
    IRoundRepository roundRepository,
    IScanLogWhitelistRepository scanLogWhitelistRepository,
    IScanLogRepository scanLogRepository)
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

        // Cập nhật trạng thái của Round thành Active nếu nó đang Pending
        // Logic này vẫn cần ở đây vì đây là nơi Round thực sự được "kích hoạt" để tính toán điểm danh.
        if (currentRound.Status == Domain.Enums.RoundStatus.Pending)
        {
            currentRound.UpdateStatus(Domain.Enums.RoundStatus.Active);
            await roundRepository.UpdateAsync(currentRound, cancellationToken);
            logger.LogInformation("Updated Round {RoundId} status to Active.", currentRound.Id);
        }

        // Gọi CalculateAndPersistRoundAttendance với RoundId từ event
        await CalculateAndPersistRoundAttendance(
            message.SessionId,
            currentRoundId, // <-- Sử dụng RoundId từ event
            cancellationToken);

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
            var attendedDeviceIds = CalculateAttendance(whitelist, scanLogs);

            // 4. Lưu kết quả điểm danh
            await PersistAttendanceResult(sessionId, roundId, attendedDeviceIds, cancellationToken);

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
        logger.LogInformation("Retrieving whitelist for Session {SessionId} using IScanLogWhitelistRepository.",
            sessionId);

        try
        {
            var whitelist = await scanLogWhitelistRepository.GetBySessionIdAsync(sessionId, cancellationToken);

            if (whitelist != null && whitelist.WhitelistedDeviceIds.Count != 0)
            {
                return [..whitelist.WhitelistedDeviceIds.Select(id => id.ToString())];
            }

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

    private List<string> CalculateAttendance(HashSet<string> whitelist, List<ScanLog> scanLogs)
    {
        logger.LogInformation("Applying BFS multi-hop attendance algorithm with whitelist of {Count} devices",
            whitelist.Count);

        logger.LogInformation("Whitelist keys: {WhitelistKeys}", string.Join(", ", whitelist));

        // 1. Filter submissions từ registered devices only
        var filteredScanLogs = FilterWhitelistSubmissions(scanLogs, whitelist);

        // 2. Build neighbor records cho mỗi device
        var deviceRecords = BuildDeviceRecords(filteredScanLogs, whitelist);

        // 3. Find lecturer làm BFS root
        var lecturerId = FindLecturerRoot(deviceRecords);

        // 4. Apply BFS algorithm (với whitelist parameter)
        var attendedDevices = ApplyBFSAlgorithm(deviceRecords, lecturerId, whitelist);

        // 5. Apply fill-in phase
        var finalAttendance = ApplyFillInPhase(deviceRecords, attendedDevices, whitelist);

        return finalAttendance.OrderBy(x => x).ToList();
    }

    /// <summary>
    /// Lọc các ScanLog chỉ từ những thiết bị đã được đăng ký (trong whitelist), về cơ bản thì khi mà phát ra
    /// thì chỉ những thằng cùng room, cùng session mới nhận ra nhau thôi. Nên bước cần là một thức cẩn thận hơn
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
    /// Xây dựng DeviceRecord cho từng thiết bị, chứa danh sách neighbors đã được tối ưu hóa cho thuật toán BFS
    /// </summary>
    /// <param name="scanLogs">Danh sách ScanLog đã được filter (chỉ từ registered devices)</param>
    /// <param name="whitelist">HashSet chứa các DeviceId đã đăng ký để filter neighbors</param>
    /// <returns>Danh sách DeviceRecord với adjacency list tối ưu cho BFS</returns>
    private List<DeviceRecord> BuildDeviceRecords(List<ScanLog> scanLogs, HashSet<string> whitelist)
    {
        logger.LogInformation("Building device records with neighbor scan lists");

        var records = scanLogs
            // Group theo DeviceId: gộp tất cả submissions của cùng 1 device
            .GroupBy(s => s.DeviceId.ToString())
            .Select(g => new DeviceRecord
            {
                // Device ID của submitter
                DeviceId = g.Key,

                // Vai trò của device (Student/Lecturer) - dùng để tìm BFS root
                Role = GetDeviceRole(g.Key),

                // Xây dựng optimized neighbor list cho device này
                ScanList = g
                    // Flatten tất cả ScannedDevices từ multiple submissions của device này
                    .SelectMany(s => s.ScannedDevices)

                    // FILTER 1: Chỉ giữ neighbors có trong whitelist (registered devices only)
                    .Where(d => whitelist.Contains(d.DeviceId))

                    // Group theo DeviceId để merge multiple scans của cùng 1 neighbor
                    .GroupBy(d => d.DeviceId)

                    // Với mỗi neighbor, lấy RSSI mạnh nhất từ multiple scans
                    .Select(gr => new { Id = gr.Key, Rssi = gr.Max(x => x.Rssi) })

                    // OPTIMIZATION 1: Sắp xếp theo signal strength (mạnh nhất trước)
                    .OrderByDescending(x => x.Rssi)

                    // OPTIMIZATION 2: Chỉ lấy top 7 neighbors mạnh nhất
                    // .Take(7) // Giới hạn để tăng performance và độ chính xác

                    // Chỉ lấy DeviceId, bỏ RSSI (không cần cho BFS)
                    .Select(x => x.Id)
                    .ToList()
            })
            .ToList();

        // Log chi tiết adjacency list của từng device để debug
        foreach (var record in records)
        {
            logger.LogInformation("Device {DeviceId} ({Role}) scanned: {Neighbors}",
                record.DeviceId, record.Role, string.Join(", ", record.ScanList));
        }

        return records;
    }

    private string FindLecturerRoot(List<DeviceRecord> deviceRecords)
    {
        var lecturerRecord = deviceRecords.FirstOrDefault(r =>
            r.Role.Equals("Lecturer", StringComparison.OrdinalIgnoreCase));

        if (lecturerRecord == null)
        {
            throw new InvalidOperationException("No lecturer found in device records for BFS root");
        }

        logger.LogInformation("Found lecturer {LecturerId} as BFS root", lecturerRecord.DeviceId);
        return lecturerRecord.DeviceId;
    }

    private HashSet<string> ApplyBFSAlgorithm(List<DeviceRecord> deviceRecords, string lecturerId,
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
            {
                if (whitelist.Contains(neighbor) && attendance.Add(neighbor))
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        logger.LogInformation("BFS phase completed: {Count} devices reached", attendance.Count);
        return attendance;
    }

    /// <summary>
    /// Fill-In Phase: Bổ sung những thiết bị bị "miss" bởi BFS nhưng thực tế đã có mặt
    ///
    /// Ý nghĩa: BFS có thể miss một số thiết bị do:
    /// - Network topology không hoàn hảo (A thấy B, nhưng B không thấy A)
    /// - Signal interference tạm thời
    /// - Timing issues (scan không đồng bộ)
    ///
    /// Fill-in logic: Nếu device X không được BFS detect, nhưng X có scan được
    /// những device đã attended → X cũng có khả năng cao đã có mặt
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
    /// Điểm danh dựa trên các device đã được phát hiện
    /// </summary>
    /// <param name="sessionId">ID của class session đang diễn ra điểm danh</param>
    /// <param name="roundId">ID của round cần lưu kết quả attendance</param>
    /// <param name="attendedDeviceIds">Danh sách DeviceId (string format) đã được BFS algorithm phát hiện có mặt</param>
    /// <param name="cancellationToken">Token để cancel operation</param>
    /// <exception cref="NotImplementedException"></exception>
    private async Task PersistAttendanceResult(Guid sessionId, Guid roundId, List<string> attendedDeviceIds,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Persisting attendance result for Round {RoundId}: {Count} devices",
            roundId, attendedDeviceIds.Count);

        // TODO: Implement saving attendance result to database
        // Create RoundAttendance entity and save via repository
        throw new NotImplementedException("PersistAttendanceResult not implemented");
    }

    private string GetDeviceRole(string deviceId)
    {
        // TODO: Implement getting device role from user/device mapping
        // Should return string like "Student", "Lecturer", "Teacher", etc.
        throw new NotImplementedException("GetDeviceRole not implemented");
    }

    // Internal DTO for algorithm processing
    private class DeviceRecord
    {
        public string DeviceId { get; set; }
        public string Role { get; set; }
        public List<string> ScanList { get; set; }
    }
}
