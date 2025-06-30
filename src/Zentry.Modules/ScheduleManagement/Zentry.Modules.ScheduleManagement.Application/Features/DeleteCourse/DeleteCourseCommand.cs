using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.DeleteCourse;

public record DeleteCourseCommand(Guid Id) : ICommand<bool>;