using MassTransit;
using Zentry.Modules.AttendanceManagement.Application.Consumers;

namespace Zentry.Modules.AttendanceManagement.Infrastructure;

public static class AttendanceMassTransitExtensions
{
    public static void AddAttendanceMassTransitConsumers(this IBusRegistrationConfigurator configurator)
    {
        configurator.AddConsumer<ProcessScanDataEventHandler>();
        configurator.AddConsumer<SessionCreatedEventHandler>();
    }

    public static void ConfigureAttendanceReceiveEndpoints(this IRabbitMqBusFactoryConfigurator cfg,
        IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint("attendance_scan_data_queue", e =>
        {
            e.ConfigureConsumer<ProcessScanDataEventHandler>(context);
            e.ConfigureConsumer<SessionCreatedEventHandler>(context);
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(10)));
        });
    }
}