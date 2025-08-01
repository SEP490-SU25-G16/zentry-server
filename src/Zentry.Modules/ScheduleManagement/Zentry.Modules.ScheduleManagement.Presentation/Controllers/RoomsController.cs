﻿using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.Modules.ScheduleManagement.Application.Features.CreateRoom;
using Zentry.Modules.ScheduleManagement.Application.Features.DeleteRoom;
using Zentry.Modules.ScheduleManagement.Application.Features.GetRoomById;
using Zentry.Modules.ScheduleManagement.Application.Features.GetRooms;
using Zentry.Modules.ScheduleManagement.Application.Features.UpdateRoom;
using Zentry.SharedKernel.Abstractions.Models;
using Zentry.SharedKernel.Extensions;

namespace Zentry.Modules.ScheduleManagement.Presentation.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController(IMediator mediator) : BaseController
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateRoomResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return HandleValidationError();
        try
        {
            var response =
                await mediator.Send(new CreateRoomCommand(request.RoomName, request.Building, request.Capacity),
                    cancellationToken);
            return HandleCreated(response, nameof(CreateRoom), new { id = response.Id });
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetRoomsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRooms([FromQuery] GetRoomsQuery query, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return HandleValidationError();
        try
        {
            var response = await mediator.Send(query, cancellationToken);
            return HandleResult(response, "Rooms retrieved successfully.");
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RoomDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoomById(Guid id, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return HandleValidationError();
        try
        {
            var query = new GetRoomByIdQuery(id);
            var response = await mediator.Send(query, cancellationToken);
            return response == null
                ? HandleNotFound("Room", id)
                : HandleResult(response, "Room retrieved successfully.");
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RoomDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRoom(Guid id, [FromBody] UpdateRoomRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return HandleValidationError();
        try
        {
            var command = new UpdateRoomCommand(id, request);
            var response = await mediator.Send(command, cancellationToken);
            return HandleResult(response, "Room updated successfully.");
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRoom(Guid id, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return HandleValidationError();
        try
        {
            var command = new DeleteRoomCommand(id);
            await mediator.Send(command, cancellationToken);
            return HandleNoContent();
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }
}
