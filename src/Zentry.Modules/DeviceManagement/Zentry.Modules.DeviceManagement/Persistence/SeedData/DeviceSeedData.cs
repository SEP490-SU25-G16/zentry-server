using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zentry.Modules.DeviceManagement.Entities;
using Zentry.Modules.DeviceManagement.ValueObjects;

// Thêm using này cho các phương thức LINQ

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

            Randomizer.Seed = new Random(400);

            var devicesToSeed = new List<Device>();
            var usedAndroidIds = new HashSet<string>();
            var faker = new Faker("vi");

            // Common device models and manufacturers
            var deviceModels = new Dictionary<string, string[]>
            {
                ["Apple"] =
                [
                    "iPhone 15 Pro", "iPhone 14", "iPhone 13 mini", "iPad Pro (M2)", "iPad Air", "AndroidIdBook Pro M3",
                    "AndroidIdBook Air M2", "iAndroidId 24-inch"
                ],
                ["Samsung"] =
                [
                    "Galaxy S24 Ultra", "Galaxy S23 FE", "Galaxy Z Fold5", "Galaxy Tab S9 Ultra", "Galaxy Book3 Pro",
                    "Galaxy Watch 6"
                ],
                ["Google"] = ["Pixel 8 Pro", "Pixel 7a", "Pixel Tablet", "Nest Hub Max", "Chromebook Pixel"],
                ["Xiaomi"] =
                    ["Xiaomi 14 Ultra", "Redmi Note 13 Pro", "Xiaomi Pad 6 Pro", "Redmi Book Pro", "Mi Band 8 Pro"],
                ["Oppo"] = ["Oppo Find X7 Ultra", "Reno11 Pro", "Oppo A98", "Oppo Pad 2"],
                ["Vivo"] = ["Vivo X100 Pro", "V29e", "Y100", "iQOO 12"],
                ["Huawei"] = ["Huawei Pura 70 Ultra", "Mate 60 Pro", "MatePad Pro", "MateBook X Pro"],
                ["OnePlus"] = ["OnePlus 12", "Nord CE 3 Lite", "OnePlus Pad", "OnePlus Watch 2"]
            };

            var platforms = new[] { "iOS", "Android", "Windows", "macOS", "iPadOS", "Chrome OS" };

            var vietnameseDeviceNames = new[]
            {
                "Điện thoại của tôi", "iPhone chính", "Máy tính bảng cá nhân", "Laptop làm việc",
                "Điện thoại dự phòng", "iPad học tập", "AndroidIdBook cá nhân", "Samsung Galaxy của tôi",
                "Xiaomi Phone", "Oppo của tôi", "Vivo của tôi", "Huawei của tôi", "OnePlus của tôi",
                "Thiết bị gia đình", "Máy tính công ty", "Tablet giải trí"
            };

            foreach (var userId in userIds)
            {
                var numberOfDevices = faker.Random.Number(DevicesPerUser, MaxDevicesPerUser);

                for (var i = 0; i < numberOfDevices; i++)
                    try
                    {
                        string androidIdValue;
                        var attempts = 0;
                        do
                        {
                            androidIdValue = GenerateRandomAndroidId(faker);
                            attempts++;
                        } while (usedAndroidIds.Contains(androidIdValue) && attempts < 100);

                        if (attempts >= 100)
                        {
                            logger?.LogWarning(
                                $"Could not generate unique Android ID after {attempts} attempts for user {userId} device {i}. Skipping this device.");
                            continue;
                        }

                        usedAndroidIds.Add(androidIdValue);

                        var manufacturer =
                            faker.PickRandom(deviceModels.Keys
                                .ToList());
                        var model = faker.PickRandom(deviceModels[manufacturer].ToList());

                        string platform;
                        if (manufacturer == "Apple")
                        {
                            if (model.Contains("iPhone")) platform = "iOS";
                            else if (model.Contains("iPad")) platform = "iPadOS";
                            else platform = "macOS";
                        }
                        else if (manufacturer == "Google")
                        {
                            platform = model.Contains("Chromebook") ? "Chrome OS" : "Android";
                        }
                        else if (manufacturer == "Samsung" && model.Contains("Galaxy Book"))
                        {
                            platform = "Windows";
                        }
                        else
                        {
                            platform = "Android";
                        }

                        platform = faker.Random
                            .ArrayElement(
                                platforms);

                        // Generate OS version based on platform
                        var osVersion = platform switch
                        {
                            "iOS" => faker.Random.ArrayElement(["17.5.1", "17.4", "17.3", "16.7.8", "16.7.7"]),
                            "iPadOS" => faker.Random.ArrayElement(
                                ["17.5.1", "17.4", "17.3", "16.7.8", "16.7.7"]),
                            "Android" => faker.Random.ArrayElement(["14", "13", "12", "11"]),
                            "Windows" => faker.Random.ArrayElement(["11", "10"]),
                            "macOS" => faker.Random.ArrayElement(["14.5", "14.4", "13.6", "12.7"]),
                            "Chrome OS" => faker.Random.ArrayElement(["126.0", "125.0", "124.0"]),
                            _ => faker.Random.ArrayElement(["1.0", "1.1", "2.0"])
                        };

                        var deviceNameValue = i == 0
                            ? faker.PickRandom(vietnameseDeviceNames)
                            : $"{model} {faker.Random.Number(1, 99)}";

                        var deviceName = DeviceName.Create(deviceNameValue);
                        var deviceToken = DeviceToken.Create();
                        var androidId = AndroidId.Create(androidIdValue);


                        var device = Device.Create(
                            userId,
                            deviceName,
                            deviceToken,
                            androidId,
                            platform,
                            osVersion,
                            model,
                            manufacturer,
                            faker.Random.ArrayElement(new[] { "1.0.0", "1.1.0", "1.2.0", "2.0.0", "2.1.0" }),
                            faker.Random.Bool(0.7f) ? GeneratePushToken(faker) : null
                        );

                        // Randomly set some devices as inactive (10% chance)
                        if (faker.Random.Bool(0.1f)) device.Deactivate();

                        devicesToSeed.Add(device);
                    }
                    catch (Exception ex)
                    {
                        logger?.LogWarning(ex, $"Failed to create device {i} for user {userId}. Error: {ex.Message}");
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

    private static string GenerateRandomAndroidId(Faker faker)
    {
        var bytes = faker.Random.Bytes(8);

        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }

    private static string GeneratePushToken(Faker faker)
    {
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