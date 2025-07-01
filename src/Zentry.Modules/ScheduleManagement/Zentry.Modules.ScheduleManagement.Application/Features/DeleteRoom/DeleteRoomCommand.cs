using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.DeleteRoom;

// Command không cần trả về gì (void), hoặc trả về bool để xác nhận thành công
public record DeleteRoomCommand(Guid Id) : ICommand<bool>; // Trả về true nếu thành công