using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.DeleteClassSection;

public record DeleteClassSectionCommand(Guid Id) : ICommand<bool>;