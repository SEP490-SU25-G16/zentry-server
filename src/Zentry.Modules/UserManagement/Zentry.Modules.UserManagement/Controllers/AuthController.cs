using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.UserManagement.Features.ResetPassword;
using Zentry.Modules.UserManagement.Features.SignIn;
using Zentry.SharedKernel.Abstractions.Models;
using Zentry.SharedKernel.Extensions;
using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.Modules.UserManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator, ISessionService sessionService) : BaseController
{
    [HttpPost("sign-in")]
    [ProducesResponseType(typeof(ApiResponse<SignInResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SignIn([FromBody] SignInCommand command)
    {
        if (!ModelState.IsValid)
            return HandleValidationError();

        try
        {
            var result = await mediator.Send(command);
            return HandleResult(result, "User signed in successfully.");
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    // ✅ THÊM: Logout API
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout()
    {
        try
        {
            // Lấy session key từ header
            var sessionKey = Request.Headers["X-Session-Key"].FirstOrDefault();
            if (string.IsNullOrEmpty(sessionKey))
            {
                return BadRequest(ApiResponse.ErrorResult("SESSION_KEY_REQUIRED", "Session key is required"));
            }

            // Revoke session
            var success = await sessionService.RevokeSessionAsync(sessionKey);
            if (success)
            {
                return HandleResult("Logged out successfully");
            }
            else
            {
                return BadRequest(ApiResponse.ErrorResult("LOGOUT_FAILED", "Failed to logout"));
            }
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPost("reset-password/request")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestResetPassword([FromBody] RequestResetPasswordCommand command)
    {
        if (!ModelState.IsValid) return HandleValidationError();
        try
        {
            await mediator.Send(command);
            // Trả về thông báo thành công chung để tránh lộ thông tin tài khoản
            return HandleResult("Password reset email sent if account exists.");
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPost("reset-password/confirm")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmResetPassword([FromBody] ConfirmResetPasswordCommand command)
    {
        if (!ModelState.IsValid) return HandleValidationError();
        try
        {
            await mediator.Send(command);
            return HandleResult("Password has been reset successfully.");
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }
}