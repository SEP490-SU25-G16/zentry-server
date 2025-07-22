using System.ComponentModel.DataAnnotations;
using Zentry.Modules.AttendanceManagement.Application.Features.SubmitScanData;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.Modules.AttendanceManagement.Domain.ValueObjects;

namespace Zentry.Modules.AttendanceManagement.Presentation.Requests;

public record SubmitScanRequest(
    [Required] Guid DeviceId, // DeviceId của thiết bị đã gửi scan
    [Required] Guid SubmitterUserId, // UserId của người dùng liên kết với DeviceId đó
    [Required] Guid SessionId,
    [Required] List<ScannedDevice> ScannedDevices,
    [Required] DateTime Timestamp
);
