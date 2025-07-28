using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zentry.Modules.DeviceManagement.Entities;
using Zentry.Modules.DeviceManagement.ValueObjects;
using Zentry.SharedKernel.Constants.Device;
using System.Linq; // Thêm using này cho các phương thức LINQ

namespace Zentry.Modules.DeviceManagement.Persistence.SeedData;

public static class DeviceSeedData
{
    private const int DevicesPerUser = 1; // Mỗi user có 2-3 devices
    private const int MaxDevicesPerUser = 1;

    public static async Task SeedAsync(DeviceDbContext context, List<Guid> userIds, ILogger? logger = null)
    {
        try
        {
            logger?.LogInformation("Starting Device Management seed data...");

            if (await context.Devices.AnyAsync())
            {
                logger?.LogInformation("Device data already exists. Skipping seed.");
                return;
            }

            if (userIds.Count == 0)
            {
                logger?.LogWarning("No User IDs available to create Devices. Please seed Users first.");
                return;
            }

            Randomizer.Seed = new Random(400); // Đặt seed để có dữ liệu nhất quán khi debug

            var devicesToSeed = new List<Device>();
            var usedMacAddresses = new HashSet<string>();
            var faker = new Faker("vi"); // Sử dụng locale tiếng Việt cho các dữ liệu phù hợp hơn

            // Common device models and manufacturers
            var deviceModels = new Dictionary<string, string[]>
            {
                ["Apple"] = ["iPhone 15 Pro", "iPhone 14", "iPhone 13 mini", "iPad Pro (M2)", "iPad Air", "MacBook Pro M3", "MacBook Air M2", "iMac 24-inch"],
                ["Samsung"] = ["Galaxy S24 Ultra", "Galaxy S23 FE", "Galaxy Z Fold5", "Galaxy Tab S9 Ultra", "Galaxy Book3 Pro", "Galaxy Watch 6"],
                ["Google"] = ["Pixel 8 Pro", "Pixel 7a", "Pixel Tablet", "Nest Hub Max", "Chromebook Pixel"],
                ["Xiaomi"] = ["Xiaomi 14 Ultra", "Redmi Note 13 Pro", "Xiaomi Pad 6 Pro", "Redmi Book Pro", "Mi Band 8 Pro"],
                ["Oppo"] = ["Oppo Find X7 Ultra", "Reno11 Pro", "Oppo A98", "Oppo Pad 2"],
                ["Vivo"] = ["Vivo X100 Pro", "V29e", "Y100", "iQOO 12"],
                ["Huawei"] = ["Huawei Pura 70 Ultra", "Mate 60 Pro", "MatePad Pro", "MateBook X Pro"],
                ["OnePlus"] = ["OnePlus 12", "Nord CE 3 Lite", "OnePlus Pad", "OnePlus Watch 2"]
            };

            var platforms = new[] { "iOS", "Android", "Windows", "macOS", "iPadOS", "Chrome OS" }; // Bổ sung Chrome OS

            // Vietnamese device names
            var vietnameseDeviceNames = new[]
            {
                "Điện thoại của tôi", "iPhone chính", "Máy tính bảng cá nhân", "Laptop làm việc",
                "Điện thoại dự phòng", "iPad học tập", "MacBook cá nhân", "Samsung Galaxy của tôi",
                "Xiaomi Phone", "Oppo của tôi", "Vivo của tôi", "Huawei của tôi", "OnePlus của tôi",
                "Thiết bị gia đình", "Máy tính công ty", "Tablet giải trí"
            };

            foreach (var userId in userIds)
            {
                var numberOfDevices = faker.Random.Number(DevicesPerUser, MaxDevicesPerUser);

                for (var i = 0; i < numberOfDevices; i++)
                {
                    try
                    {
                        // Generate unique MAC address
                        string macAddressValue;
                        var attempts = 0;
                        do
                        {
                            macAddressValue = GenerateRandomMacAddress(faker);
                            attempts++;
                        } while (usedMacAddresses.Contains(macAddressValue) && attempts < 100);

                        if (attempts >= 100)
                        {
                            logger?.LogWarning($"Could not generate unique MAC address after {attempts} attempts for user {userId} device {i}. Skipping this device.");
                            continue;
                        }

                        usedMacAddresses.Add(macAddressValue);

                        // Select random manufacturer and model
                        var manufacturer = faker.PickRandom(deviceModels.Keys.ToList()); // Chuyển Keys sang List để PickRandom hoạt động đúng
                        var model = faker.PickRandom(deviceModels[manufacturer].ToList()); // Chuyển mảng sang List

                        // Determine platform based on manufacturer and model
                        string platform;
                        if (manufacturer == "Apple")
                        {
                            if (model.Contains("iPhone")) platform = "iOS";
                            else if (model.Contains("iPad")) platform = "iPadOS";
                            else platform = "macOS";
                        }
                        else if (manufacturer == "Google")
                        {
                            if (model.Contains("Chromebook")) platform = "Chrome OS";
                            else platform = "Android";
                        }
                        else if (manufacturer == "Samsung" && model.Contains("Galaxy Book"))
                        {
                            platform = "Windows";
                        }
                        else
                        {
                            // Đối với các hãng khác, phần lớn là Android
                            platform = "Android";
                        }
                        // Đảm bảo platform là một trong các giá trị đã định nghĩa
                        platform = faker.Random.ArrayElement(platforms); // Chọn lại ngẫu nhiên từ danh sách platforms đã định nghĩa để đa dạng hơn

                        // Generate OS version based on platform
                        var osVersion = platform switch
                        {
                            "iOS" => faker.Random.ArrayElement(new[] { "17.5.1", "17.4", "17.3", "16.7.8", "16.7.7" }),
                            "iPadOS" => faker.Random.ArrayElement(new[] { "17.5.1", "17.4", "17.3", "16.7.8", "16.7.7" }),
                            "Android" => faker.Random.ArrayElement(new[] { "14", "13", "12", "11" }),
                            "Windows" => faker.Random.ArrayElement(new[] { "11", "10" }),
                            "macOS" => faker.Random.ArrayElement(new[] { "14.5", "14.4", "13.6", "12.7" }),
                            "Chrome OS" => faker.Random.ArrayElement(new[] { "126.0", "125.0", "124.0" }),
                            _ => faker.Random.ArrayElement(new[] { "1.0", "1.1", "2.0" }) // Fallback cho các platform không xác định
                        };

                        // Create device name (mix Vietnamese and English)
                        var deviceNameValue = i == 0 ?
                            faker.PickRandom(vietnameseDeviceNames) :
                            $"{model} {faker.Random.Number(1, 99)}";

                        // Tạo ValueObjects đúng cách
                        var deviceName = DeviceName.Create(deviceNameValue); // Khởi tạo DeviceName ValueObject
                        var deviceToken = DeviceToken.Create(); // Nếu DeviceToken.Create() tạo ngẫu nhiên
                                                              // Nếu không, bạn cần tạo string ngẫu nhiên và truyền vào:
                                                              // var deviceToken = DeviceToken.Create(GeneratePushToken(faker));
                        var macAddress = MacAddress.Create(macAddressValue); // Khởi tạo MacAddress ValueObject


                        var device = Device.Create(
                            userId,
                            deviceName,
                            deviceToken,
                            macAddress, // Truyền MacAddress ValueObject
                            platform,
                            osVersion,
                            model,
                            manufacturer,
                            faker.Random.ArrayElement(new[] { "1.0.0", "1.1.0", "1.2.0", "2.0.0", "2.1.0" }),
                            faker.Random.Bool(0.7f) ? GeneratePushToken(faker) : null
                        );

                        // Randomly set some devices as inactive (10% chance)
                        if (faker.Random.Bool(0.1f))
                        {
                            device.Deactivate();
                        }

                        devicesToSeed.Add(device);
                    }
                    catch (Exception ex)
                    {
                        logger?.LogWarning(ex, $"Failed to create device {i} for user {userId}. Error: {ex.Message}");
                    }
                }
            }

            if (devicesToSeed.Count > 0)
            {
                await context.Devices.AddRangeAsync(devicesToSeed);
                logger?.LogInformation($"Added {devicesToSeed.Count} devices for {userIds.Count} users.");
            }
            else
            {
                logger?.LogWarning("No devices were generated during seeding.");
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while seeding Device data");
            throw;
        }
    }

    private static string GenerateRandomMacAddress(Faker faker)
    {
        var bytes = faker.Random.Bytes(6);

        // Ensure it's unicast and globally unique (clear multicast and locally administered bits)
        // Đây là cách phổ biến để tạo MAC cho các mục đích test/internal, không nên dùng cho các thiết bị thật
        bytes[0] = (byte)(bytes[0] & 0xFC); // Clear multicast (bit 0) and locally administered (bit 1)

        return string.Join(":", bytes.Select(b => b.ToString("X2")));
    }

    private static string GeneratePushToken(Faker faker)
    {
        // Generate a realistic push notification token (similar to FCM/APNS tokens)
        var tokenBytes = faker.Random.Bytes(32);
        return Convert.ToBase64String(tokenBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    public static async Task ClearAllData(DeviceDbContext context, ILogger? logger = null)
    {
        logger?.LogInformation("Clearing all Device Management data...");
        context.Devices.RemoveRange(context.Devices);
        await context.SaveChangesAsync();
        logger?.LogInformation("All Device Management data cleared.");
    }

    public static async Task ReseedAsync(DeviceDbContext context, List<Guid> userIds, ILogger? logger = null)
    {
        await ClearAllData(context, logger);
        await SeedAsync(context, userIds, logger);
    }
}
