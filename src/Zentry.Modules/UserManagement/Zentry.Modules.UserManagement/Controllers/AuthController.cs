using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.UserManagement.Features.ResetPassword;
using Zentry.Modules.UserManagement.Features.SignIn;

namespace Zentry.Modules.UserManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn([FromBody] SignInCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("reset-password/request")]
    public async Task<IActionResult> RequestResetPassword([FromBody] RequestResetPasswordCommand command)
    {
        await mediator.Send(command);
        return Ok("Password reset email sent if account exists.");
    }

    [HttpPost("reset-password/confirm")]
    public async Task<IActionResult> ConfirmResetPassword([FromBody] ConfirmResetPasswordCommand command)
    {
        await mediator.Send(command);
        return Ok("Password has been reset successfully.");
    }
}