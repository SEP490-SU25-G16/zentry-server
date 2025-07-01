using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.DeviceManagement.Application.Features.RegisterDevice;
using Zentry.Modules.DeviceManagement.Presentation.Requests;
using Zentry.SharedKernel.Exceptions;

// Assuming RegisterDeviceRequest is here
// For [Authorize] attribute
// For ClaimTypes

namespace Zentry.Modules.DeviceManagement.Presentation.Controllers;

[ApiController]
[Route("api/devices")]
[Authorize] // Apply authorization to ensure only authenticated users can access
public class DevicesController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterDeviceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // For BusinessLogicException
    [ProducesResponseType(StatusCodes.Status404NotFound)] // For UserNotFound if applicable, or Handled by 400
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // If [Authorize] fails
    public async Task<IActionResult> Register([FromBody] RegisterDeviceRequest request,
        CancellationToken cancellationToken)
    {
        // 1. Lấy UserId từ JWT (đã được middleware xác thực và gán vào HttpContext.User)
        // Đây là cách an toàn và chuẩn để lấy UserId của người dùng đã đăng nhập.
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            // Trường hợp này hiếm khi xảy ra nếu Authorization attribute hoạt động đúng,
            // nhưng là một kiểm tra an toàn nếu JWT hợp lệ nhưng không có claim User ID.
            return Unauthorized("User ID claim not found or invalid in token.");

        // 2. Tạo RegisterDeviceCommand và gán UserId
        // Command sẽ chứa tất cả thông tin cần thiết cho handler.
        var command = new RegisterDeviceCommand
        {
            UserId = userId,
            DeviceName = request.DeviceName
            // Các trường DeviceModel, OperatingSystem, OSVersion, AppVersion đã bị loại bỏ theo quyết định của bạn.
        };

        try
        {
            // 3. Gửi command tới MediatR để được xử lý bởi RegisterDeviceCommandHandler
            var response = await mediator.Send(command, cancellationToken); //

            // 4. Trả về phản hồi thành công (HTTP 201 Created)
            // Cung cấp URL cho resource mới tạo nếu cần, hoặc đơn giản là trả về response
            return CreatedAtAction(nameof(Register), new { id = response.DeviceId }, response); //
        }
        catch (BusinessLogicException ex)
        {
            // Bắt các ngoại lệ nghiệp vụ cụ thể (như "User already has a primary device registered.")
            // và trả về lỗi 400 Bad Request kèm thông báo.
            return BadRequest(new { message = ex.Message }); //
        }
        // catch (NotFoundException ex) // If you introduce a specific NotFoundException for user not found
        // {
        //     return NotFound(new { message = ex.Message });
        // }
        catch (Exception ex)
        {
            // Bắt các ngoại lệ không mong muốn khác và trả về 500 Internal Server Error
            // Quan trọng: Ghi log lỗi chi tiết ở đây!
            // Logger.LogError(ex, "An unhandled error occurred during device registration.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An unexpected error occurred." });
        }
    }
}