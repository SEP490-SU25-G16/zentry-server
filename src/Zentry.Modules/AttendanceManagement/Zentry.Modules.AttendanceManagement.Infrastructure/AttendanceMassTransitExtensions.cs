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

        configurator.AddConsumer<CalculateRoundAttendanceConsumer>();
        configurator.AddConsumer<ProcessActiveRoundForEndSessionConsumer>();
    }

    public static void ConfigureAttendanceReceiveEndpoints(this IRabbitMqBusFactoryConfigurator cfg,
        IBusRegistrationContext context)
    {
        // Main attendance processing queue
        cfg.ReceiveEndpoint("attendance_scan_data_queue", e =>
        {
            e.ConfigureConsumer<CreateRoundsConsumer>(context);
            e.ConfigureConsumer<CreateSessionConsumer>(context);
            e.ConfigureConsumer<GenerateSessionWhitelistConsumer>(context);
            e.ConfigureConsumer<SubmitScanDataConsumer>(context);
            e.ConfigureConsumer<FinalAttendanceConsumer>(context);
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(10)));
        });

        cfg.ReceiveEndpoint("attendance_calculation_queue", e =>
        {
            e.ConfigureConsumer<CalculateRoundAttendanceConsumer>(context);

            e.UseMessageRetry(r =>
            {
                r.Interval(5, TimeSpan.FromSeconds(30));
                r.Handle<InvalidOperationException>();
                r.Handle<ArgumentException>();
                r.Handle<DivideByZeroException>();
            });

            e.UseCircuitBreaker(cb =>
            {
                cb.TrackingPeriod = TimeSpan.FromMinutes(1);
                cb.TripThreshold = 15;
                cb.ActiveThreshold = 10;
                cb.ResetInterval = TimeSpan.FromMinutes(5);
            });
        });

        cfg.ReceiveEndpoint("end_session_processing_queue", e =>
        {
            e.ConfigureConsumer<ProcessActiveRoundForEndSessionConsumer>(context);

            e.UseMessageRetry(r =>
            {
                r.Interval(10, TimeSpan.FromSeconds(45));
                r.Handle<InvalidOperationException>();
                r.Handle<ArgumentException>();
                r.Handle<DivideByZeroException>();
            });

            e.UseCircuitBreaker(cb =>
            {
                cb.TrackingPeriod = TimeSpan.FromMinutes(1);
                cb.TripThreshold = 10;
                cb.ActiveThreshold = 5;
                cb.ResetInterval = TimeSpan.FromMinutes(5);
            });
        });
    }
}