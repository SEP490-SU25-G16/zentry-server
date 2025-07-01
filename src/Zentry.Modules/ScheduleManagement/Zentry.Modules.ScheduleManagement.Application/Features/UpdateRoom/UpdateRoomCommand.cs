using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.UpdateRoom;

// Record để tạo Command immutable. Id là từ URL, các trường còn lại từ Body.
public record UpdateRoomCommand(
    Guid Id, // ID của phòng học cần cập nhật
    string RoomName,
    string Building,
    int Capacity
) : ICommand<RoomDetailDto>; // Trả về DTO của phòng học đã cập nhật