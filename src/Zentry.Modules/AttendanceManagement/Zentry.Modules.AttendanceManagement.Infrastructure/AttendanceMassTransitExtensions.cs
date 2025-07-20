using MassTransit;
using Zentry.Modules.AttendanceManagement.Application.EventHandlers;

namespace Zentry.Modules.AttendanceManagement.Infrastructure;

public static class AttendanceMassTransitExtensions
{
    public static void AddAttendanceMassTransitConsumers(this IBusRegistrationConfigurator configurator)
    {
        configurator.AddConsumer<ProcessScanDataMessageConsumer>();
        configurator.AddConsumer<CreateRoundMessageConsumer>();
    }

    public static void ConfigureAttendanceReceiveEndpoints(this IRabbitMqBusFactoryConfigurator cfg,
        IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint("attendance_scan_data_queue", e =>
        {
            e.ConfigureConsumer<ProcessScanDataMessageConsumer>(context);
            e.ConfigureConsumer<CreateRoundMessageConsumer>(context);
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(10)));
        });
    }
}
