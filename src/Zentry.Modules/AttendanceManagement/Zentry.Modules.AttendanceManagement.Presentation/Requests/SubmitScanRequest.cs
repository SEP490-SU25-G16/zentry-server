using System.ComponentModel.DataAnnotations;
using Zentry.Modules.AttendanceManagement.Application.Features.SubmitScanData;

namespace Zentry.Modules.AttendanceManagement.Presentation.Requests;

public record SubmitScanRequest(
    [Required] Guid DeviceId, // DeviceId của thiết bị đã gửi scan
    [Required] Guid SubmitterUserId, // UserId của người dùng liên kết với DeviceId đó
    [Required] Guid SessionId,
    [Required] List<ScannedDevice> ScannedDevices,
    [Required] DateTime Timestamp
);
