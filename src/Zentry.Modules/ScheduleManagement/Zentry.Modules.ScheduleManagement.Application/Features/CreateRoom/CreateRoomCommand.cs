using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.CreateRoom;

public record CreateRoomCommand(
    string RoomName,
    string Building,
    int Capacity
) : ICommand<RoomCreatedResponseDto>;