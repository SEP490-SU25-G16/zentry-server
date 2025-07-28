using System.ComponentModel.DataAnnotations;

namespace Zentry.Modules.AttendanceManagement.Domain.ValueObjects;

public record ScannedDeviceFromRequest(
    [Required] string MacAddress,
    [Required] int Rssi
);
