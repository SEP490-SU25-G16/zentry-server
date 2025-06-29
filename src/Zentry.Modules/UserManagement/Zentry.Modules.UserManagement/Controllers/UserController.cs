using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.UserManagement.Features.CreateUser;
using Zentry.Modules.UserManagement.Features.DeleteUser;
using Zentry.Modules.UserManagement.Features.GetUser;
using Zentry.Modules.UserManagement.Features.GetUsers;
using Zentry.Modules.UserManagement.Features.UpdateUser;
// Thêm using MediatR

// Thêm using này nếu chưa có

namespace Zentry.Modules.UserManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IMediator mediator) : ControllerBase
{
    [HttpGet] // HTTP GET không có ID trong URL path để lấy danh sách
    [ProducesResponseType(typeof(GetUsersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query) // [FromQuery] để lấy từ query string
    {
        try
        {
            var response = await mediator.Send(query);
            return Ok(response);
        }
        catch (Exception ex)
        {
            // Ghi log lỗi
            // Logger.LogError(ex, "Error getting list of users.");
            return BadRequest(new { message = "An error occurred while retrieving the list of users." });
        }
    }

    [HttpGet("{id}")] // HTTP GET để lấy thông tin
    [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserDetail(Guid id)
    {
        var query = new GetUserQuery(id);

        try
        {
            var response = await mediator.Send(query);

            if (response == null) return NotFound(new { message = $"User with ID '{id}' not found." });

            return Ok(response);
        }
        catch (Exception ex)
        {
            // Ghi log lỗi
            // Logger.LogError(ex, "Error getting user detail for ID: {UserId}", id);
            return BadRequest(new { message = "An error occurred while retrieving user details." });
        }
    }

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
                    return NotFound(new { message = response.Message });

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

    [HttpDelete("{id}")] // HTTP DELETE để xóa (soft delete)
    [ProducesResponseType(typeof(DeleteUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SoftDeleteUser(Guid id)
    {
        var command = new DeleteUserCommand(id);

        try
        {
            var response = await mediator.Send(command);

            if (!response.Success)
            {
                if (response.Message.Contains("not found")) // Kiểm tra tin nhắn để phân biệt lỗi
                    return NotFound(new { message = response.Message });
                return BadRequest(new { message = response.Message });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            // Ghi log lỗi
            // Logger.LogError(ex, "Error soft deleting user with ID: {UserId}", id);
            return BadRequest(new { message = "An unexpected error occurred during soft delete." });
        }
    }
}
