using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.ScheduleManagement.Application.Dtos;
using Zentry.Modules.ScheduleManagement.Application.Features.CreateRoom;
using Zentry.Modules.ScheduleManagement.Application.Features.DeleteRoom;
using Zentry.Modules.ScheduleManagement.Application.Features.GetRoomById;
using Zentry.Modules.ScheduleManagement.Application.Features.GetRooms;
using Zentry.Modules.ScheduleManagement.Application.Features.UpdateRoom;

namespace Zentry.Modules.ScheduleManagement.Presentation.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(RoomCreatedResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomCommand request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var response = await mediator.Send(request, cancellationToken);

        return CreatedAtAction(nameof(CreateRoom), new { id = response.Id }, response);
    }

    [HttpGet] // Endpoint để lấy danh sách phòng học với phân trang
    [ProducesResponseType(typeof(GetRoomsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRooms([FromQuery] GetRoomsQuery query, CancellationToken cancellationToken)
    {
        // Model binding sẽ tự động điền các thuộc tính của GetRoomsQuery từ query string
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var response = await mediator.Send(query, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")] // Định tuyến để nhận ID là một GUID
    [ProducesResponseType(typeof(RoomDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoomById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetRoomByIdQuery(id);

        try
        {
            var response = await mediator.Send(query, cancellationToken);
            return Ok(response); // Trả về 200 OK nếu tìm thấy
        }
        catch (Exception ex)
        {
            // Bắt NotFoundException và trả về 404 Not Found
            return NotFound(new { errors = ex.Message });
        }
    }

    [HttpPut("{id:guid}")] // Định tuyến PUT với ID từ URL
    [ProducesResponseType(typeof(RoomDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Invalid input or business logic error
    [ProducesResponseType(StatusCodes.Status404NotFound)] // Room not found
    public async Task<IActionResult> UpdateRoom(Guid id, [FromBody] UpdateRoomCommand request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Tạo một bản sao của request với Id từ URL, đảm bảo Id trong command khớp với Id trong route
        var command = request with { Id = id };

        try
        {
            var response = await mediator.Send(command, cancellationToken);
            return Ok(response); // Trả về 200 OK với thông tin phòng học đã cập nhật
        }
        catch (BadHttpRequestException ex)
        {
            return NotFound(new { errors = ex.Message });
        }
        catch (Exception ex) // Bắt các lỗi nghiệp vụ khác
        {
            return BadRequest(new { errors = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")] // Endpoint này giữ nguyên
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRoom(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteRoomCommand(id);

        try
        {
            await mediator.Send(command, cancellationToken);
            return NoContent(); // Trả về 204 No Content khi xóa thành công
        }
        catch (BadHttpRequestException ex)
        {
            return NotFound(new { errors = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { errors = ex.Message });
        }
    }
}