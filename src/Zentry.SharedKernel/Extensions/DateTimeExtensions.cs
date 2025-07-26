using System;
using TimeZoneConverter; // Đảm bảo đã cài đặt NuGet package này

public static class DateTimeExtensions
{
    // Múi giờ mặc định cho Việt Nam (Asia/Ho_Chi_Minh là ID IANA chuẩn)
    private static readonly TimeZoneInfo VietnamTimeZone =
        TZConvert.GetTimeZoneInfo("Asia/Ho_Chi_Minh");

    /// <summary>
    /// Chuyển đổi DateTime UTC sang giờ địa phương Việt Nam.
    /// </summary>
    public static DateTime ToVietnamLocalTime(this DateTime utcDateTime)
    {
        // Nếu đã là Local hoặc Unspecified, chuyển về UTC trước khi chuyển đổi
        if (utcDateTime.Kind == DateTimeKind.Local)
        {
            utcDateTime = utcDateTime.ToUniversalTime();
        }
        else if (utcDateTime.Kind == DateTimeKind.Unspecified)
        {
            utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        }
        // Nếu đã là UTC, giữ nguyên

        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, VietnamTimeZone);
    }

    /// <summary>
    /// Chuyển đổi DateTime đại diện cho giờ địa phương Việt Nam sang UTC.
    /// Đầu vào `localDateTime` nên có Kind là `Unspecified` hoặc `Local` (sẽ được xử lý).
    /// </summary>
    public static DateTime ToUtcFromVietnamLocalTime(this DateTime localDateTime)
    {
        // Nếu đã là UTC, không cần làm gì nữa
        if (localDateTime.Kind == DateTimeKind.Utc)
        {
            return localDateTime;
        }

        // Nếu là DateTimeKind.Local (nghĩa là múi giờ của hệ thống server),
        // chúng ta cần chuyển đổi nó sang UTC trước khi áp dụng múi giờ Việt Nam.
        // Đây là để tránh xung đột với TimeZoneInfo.Local
        if (localDateTime.Kind == DateTimeKind.Local)
        {
            // Convert it to UTC based on the system's local time zone
            localDateTime = localDateTime.ToUniversalTime();
            // Sau khi ToUniversalTime, Kind sẽ là Utc, nên sẽ thoát ở if đầu tiên.
            // Để nó xử lý tiếp, chúng ta cần biến nó về Unspecified
            localDateTime = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
        }

        // Tại đây, localDateTime có Kind = Unspecified và giá trị là giờ cục bộ Việt Nam.
        // Chúng ta muốn coi giá trị này là giờ của VietnamTimeZone và chuyển nó về UTC.
        // Cách tốt nhất là sử dụng TimeZoneInfo.ConvertTimeToUtc với DateTimeKind.Unspecified
        // và TimeZoneInfo cụ thể.
        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, VietnamTimeZone);
    }
}
