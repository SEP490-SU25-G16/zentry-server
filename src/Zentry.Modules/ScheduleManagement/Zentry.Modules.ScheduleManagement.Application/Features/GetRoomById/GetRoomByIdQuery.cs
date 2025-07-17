using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.GetRoomById;

// Sử dụng record để tạo Query immutable
public record GetRoomByIdQuery(Guid Id) : IQuery<RoomDto?>; // Trả về RoomDetailDto hoặc null nếu không tìm thấy
