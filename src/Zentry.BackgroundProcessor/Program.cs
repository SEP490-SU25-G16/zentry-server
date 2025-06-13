using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zentry.BackgroundProcessor.Workers;
using Zentry.Infrastructure.Messaging.External;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<AttendanceProcessingWorker>();
        services.AddSingleton<MessagePublisher>();
    })
    .Build();

await host.RunAsync();
