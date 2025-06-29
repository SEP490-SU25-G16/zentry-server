using MediatR;
using Microsoft.AspNetCore.Http; // Thêm using MediatR
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.UserManagement.Features.CreateUser;
using Zentry.Modules.UserManagement.Features.UpdateUser; // Thêm using này nếu chưa có

namespace Zentry.Modules.UserManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IMediator mediator) : ControllerBase
{
    [HttpPost("create-user")]
    [ProducesResponseType(typeof(CreateUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        // Tạo Command từ Request
        var command = new CreateUserCommand(request);

        try
        {
            // Gửi Command qua Mediator để Handler xử lý
            var response = await mediator.Send(command);
            return CreatedAtAction(nameof(CreateUser), new { id = response.UserId }, response);
        }
        catch (InvalidOperationException ex) // Bắt lỗi nếu email đã tồn tại
        {
            return Conflict(new { message = ex.Message }); // Trả về 409 Conflict
        }
        catch (Exception ex)
        {
            // Ghi log lỗi và trả về lỗi chung
            // Logger.LogError(ex, "Error creating user");
            return BadRequest(new { message = "An error occurred while creating the user." });
        }
    }

    [HttpPut("{id}")] // PUT để cập nhật toàn bộ tài nguyên, hoặc PATCH để cập nhật một phần
    [ProducesResponseType(typeof(UpdateUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        // Tạo Command từ Request và thêm User ID từ URL
        var command = new UpdateUserCommand(id, request);

        try
        {
            // Gửi Command qua Mediator để Handler xử lý
            var response = await mediator.Send(command);

            if (!response.Success)
            {
                if (response.Message == "User not found." || response.Message == "Associated account not found.")
                {
                    return NotFound(new { message = response.Message });
                }
                return BadRequest(new { message = response.Message });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            // Ghi log lỗi và trả về lỗi chung
            // Logger.LogError(ex, "Error updating user with ID: {UserId}", id);
            return BadRequest(new { message = "An error occurred while updating the user." });
        }
    }
}
