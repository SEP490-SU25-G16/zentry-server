using MassTransit;
using Zentry.Modules.AttendanceManagement.Infrastructure.Consumers;

namespace Zentry.Modules.AttendanceManagement.Infrastructure;

public static class AttendanceMassTransitExtensions
{
    public static void AddAttendanceMassTransitConsumers(this IBusRegistrationConfigurator configurator)
    {
        // Đăng ký Consumer cho Attendance module
        configurator.AddConsumer<ProcessScanDataMessageConsumer>();
    }

    public static void ConfigureAttendanceReceiveEndpoints(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {
        // Cấu hình Receive Endpoint cụ thể cho Attendance module
        cfg.ReceiveEndpoint("attendance_scan_data_queue", e =>
        {
            e.ConfigureConsumer<ProcessScanDataMessageConsumer>(context);
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(10)));
            // Thêm các cấu hình MassTransit khác liên quan đến Attendance module tại đây
        });
    }
}
