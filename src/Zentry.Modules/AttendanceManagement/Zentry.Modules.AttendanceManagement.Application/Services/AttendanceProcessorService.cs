using Microsoft.Extensions.Logging;
using Zentry.Modules.AttendanceManagement.Application.Abstractions;
using Zentry.Modules.AttendanceManagement.Application.Services.Interface;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Contracts.Messages;

namespace Zentry.Modules.AttendanceManagement.Application.Services;

public class AttendanceProcessorService(
    ILogger<AttendanceProcessorService> logger,
    IRoundRepository roundRepository)
    : IAttendanceProcessorService
{
    public async Task ProcessBluetoothScanData(ProcessScanDataMessage message, CancellationToken cancellationToken)
    {
        Round? currentRound = null;
        try
        {
            // Lấy tất cả các Round của Session này từ DB
            var allRoundsInSession =
                await roundRepository.GetRoundsBySessionIdAsync(message.SessionId, cancellationToken);

            // Tìm Round mà Timestamp của message nằm trong khoảng StartTime và EndTime của Round
            // Nếu EndTime là null, coi như Round đó vẫn đang Active.
            // Loại bỏ RoundStatus.Expired vì nó không có trong Enum của bạn.
            currentRound = allRoundsInSession
                .Where(r => message.Timestamp >= r.StartTime &&
                            (r.EndTime == null || message.Timestamp <= r.EndTime) &&
                            r.Status != Domain.Enums.RoundStatus.Completed &&
                            r.Status != Domain.Enums.RoundStatus.Cancelled && // Thêm Cancelled
                            r.Status != Domain.Enums.RoundStatus.Finalized) // Thêm Finalized
                .OrderByDescending(r => r.StartTime) // Nếu có nhiều round khớp (trường hợp hiếm), lấy round mới nhất
                .FirstOrDefault();

            if (currentRound == null)
            {
                logger.LogWarning(
                    "No active round found for Session {SessionId} at timestamp {Timestamp}. Scan data logged but not used for attendance calculation in a specific round.",
                    message.SessionId, message.Timestamp);
                // Dừng xử lý attendance cho round nếu không tìm thấy round phù hợp
                return;
            }

            logger.LogInformation(
                "Scan data for Session {SessionSessionId} assigned to Round {RoundId} (RoundNumber: {RoundNumber}).",
                message.SessionId, currentRound.Id, currentRound.RoundNumber);

            // Cập nhật trạng thái của Round thành Active nếu nó đang Pending
            // Dựa trên RoundStatus Enum của bạn, 'Active' là trạng thái phù hợp khi Round bắt đầu nhận dữ liệu.
            if (currentRound.Status == Domain.Enums.RoundStatus.Pending)
            {
                currentRound.UpdateStatus(Domain.Enums.RoundStatus.Active); // Sử dụng RoundStatus.Active
                await roundRepository.UpdateAsync(currentRound, cancellationToken); // Sử dụng UpdateAsync
                logger.LogInformation("Updated Round {RoundId} status to Active.", currentRound.Id);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error finding active round for Session {SessionId} at timestamp {Timestamp}.",
                message.SessionId, message.Timestamp);
            // Log lỗi và có thể re-throw hoặc xử lý tùy vào nghiệp vụ
            throw new ApplicationException("Error determining active round for scan data.", ex);
        }

        if (currentRound != null)
        {
            await CalculateAndPersistRoundAttendance(
                message.SessionId,
                currentRound.Id,
                cancellationToken);
        }

        logger.LogInformation("Finished processing scan data for SessionId: {SessionId}.", message.SessionId);
    }

    private async Task CalculateAndPersistRoundAttendance(
        Guid sessionId,
        Guid roundId,
        CancellationToken cancellationToken)
    {
        // 5.2. Lấy Whitelist (Dữ liệu User/Thiết bị)
        logger.LogInformation("Placeholder for: Retrieving Whitelist (User/Device data).");
        // 5.3. Truy vấn ScanLogs cho Round:
        logger.LogInformation("Placeholder for: Querying ScanLogs for Round {RoundId}.", roundId);
        // 5.4. Áp dụng Thuật toán Điểm danh (BFS Multi-hop)
        logger.LogInformation("Placeholder for: Applying BFS Multi-hop Attendance Algorithm.");
        // 5.5. Lưu Kết quả Điểm danh
        logger.LogInformation("Placeholder for: Saving Round Attendance Result to Database.");
    }
}
