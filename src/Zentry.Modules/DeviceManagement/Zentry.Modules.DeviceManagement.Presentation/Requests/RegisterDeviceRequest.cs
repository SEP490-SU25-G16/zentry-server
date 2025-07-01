namespace Zentry.Modules.DeviceManagement.Presentation.Requests;

public class RegisterDeviceRequest
{
    // Đây là tên thiết bị do người dùng nhập vào (ví dụ: "Điện thoại của tôi")
    public string DeviceName { get; set; } = string.Empty;

    // Các trường DeviceModel, OperatingSystem, OSVersion, AppVersion đã được loại bỏ.
}