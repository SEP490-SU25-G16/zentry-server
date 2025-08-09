﻿using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.ScheduleManagement.Application.Features.Rooms.CreateRoom;

public record CreateRoomCommand(
    string RoomName,
    string Building,
    int Capacity
) : ICommand<CreateRoomResponse>;

public class CreateRoomResponse
{
    public Guid Id { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public DateTime CreatedAt { get; set; }
}