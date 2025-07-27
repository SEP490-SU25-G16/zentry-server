using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.DeviceManagement.Features.RegisterDevice;
using Zentry.SharedKernel.Abstractions.Models;
using Zentry.SharedKernel.Extensions;

// Assuming RegisterDeviceRequest is here
// For [Authorize] attribute
// For ClaimTypes

namespace Zentry.Modules.DeviceManagement.Controllers;

[ApiController]
[Route("api/devices")]
// [Authorize] // Apply authorization to ensure only authenticated users can access
public class DevicesController(IMediator mediator) : BaseController
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<RegisterDeviceResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)] // For BusinessLogicException
    [ProducesResponseType(typeof(ApiResponse),
        StatusCodes.Status404NotFound)] // For UserNotFound if applicable, or Handled by 400
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)] // If [Authorize] fails
    public async Task<IActionResult> Register([FromBody] RegisterDeviceRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return HandleValidationError();

        // 1. Lấy UserId từ JWT (đã được middleware xác thực và gán vào HttpContext.User)
        // Đây là cách an toàn và chuẩn để lấy UserId của người dùng đã đăng nhập.
        // var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        // {
        //     // Trường hợp này hiếm khi xảy ra nếu Authorization attribute hoạt động đúng,
        //     // nhưng là một kiểm tra an toàn nếu JWT hợp lệ nhưng không có claim User ID.
        //     return Unauthorized("User ID claim not found or invalid in token."); // Có thể chuyển thành HandleError
        // }
        var userId = request.UserId; // Sử dụng userId trực tiếp từ request như bạn đang làm cho testing

        // 2. Tạo RegisterDeviceCommand và gán UserId cùng với các thông tin thiết bị bổ sung
        var command = new RegisterDeviceCommand
        {
            UserId = new Guid(userId),
            DeviceName = request.DeviceName,
            Platform = request.Platform,
            OsVersion = request.OsVersion,
            Model = request.Model,
            Manufacturer = request.Manufacturer,
            AppVersion = request.AppVersion,
            PushNotificationToken = request.PushNotificationToken
        };

        try
        {
            // 3. Gửi command tới MediatR để được xử lý bởi RegisterDeviceCommandHandler
            var response = await mediator.Send(command, cancellationToken);
            // 4. Trả về phản hồi thành công (HTTP 201 Created)
            // Cung cấp URL cho resource mới tạo nếu cần, hoặc đơn giản là trả về response
            return HandleCreated(response, nameof(Register), new { id = response.DeviceId });
        }
        catch (Exception ex)
        {
            // Sử dụng HandleError để xử lý các loại ngoại lệ đã định nghĩa
            return HandleError(ex);
        }
    }
}