using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.SharedKernel.Contracts.Schedule;

public record GetScheduleByIdIntegrationQuery(Guid Id) : IQuery<GetScheduleByIdIntegrationResponse>;
