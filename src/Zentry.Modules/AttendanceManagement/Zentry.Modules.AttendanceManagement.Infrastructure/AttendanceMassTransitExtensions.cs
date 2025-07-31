using MassTransit;
using Zentry.Modules.AttendanceManagement.Application.EventHandlers;

namespace Zentry.Modules.AttendanceManagement.Infrastructure;

public static class AttendanceMassTransitExtensions
{
    public static void AddAttendanceMassTransitConsumers(this IBusRegistrationConfigurator configurator)
    {
        configurator.AddConsumer<CreateRoundsConsumer>();
        configurator.AddConsumer<CreateSessionConsumer>();
        configurator.AddConsumer<GenerateSessionWhitelistConsumer>();
        configurator.AddConsumer<SubmitScanDataConsumer>();
        configurator.AddConsumer<FinalAttendanceConsumer>();
    }

    public static void ConfigureAttendanceReceiveEndpoints(this IRabbitMqBusFactoryConfigurator cfg,
        IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint("attendance_scan_data_queue", e =>
        {
            e.ConfigureConsumer<CreateRoundsConsumer>(context);
            e.ConfigureConsumer<CreateSessionConsumer>(context);
            e.ConfigureConsumer<GenerateSessionWhitelistConsumer>(context);
            e.ConfigureConsumer<SubmitScanDataConsumer>(context);
            e.ConfigureConsumer<FinalAttendanceConsumer>(context);
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(10)));
        });
    }
}