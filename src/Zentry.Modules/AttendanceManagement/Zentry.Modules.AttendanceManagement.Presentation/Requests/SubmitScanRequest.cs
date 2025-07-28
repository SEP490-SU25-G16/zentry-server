using System.ComponentModel.DataAnnotations;
using Zentry.Modules.AttendanceManagement.Domain.ValueObjects;

namespace Zentry.Modules.AttendanceManagement.Presentation.Requests;


public record SubmitScanRequest(
    [Required] string SubmitterDeviceMacAddress, // Đổi từ DeviceId thành MacAddress của thiết bị gửi request
    // [Required] Guid SubmitterUserId, // UserId sẽ được lấy từ MacAddress
    [Required] Guid SessionId,
    [Required] List<ScannedDeviceFromRequest> ScannedDevices, // Thay đổi loại ScannedDevice
    [Required] DateTime Timestamp
);
