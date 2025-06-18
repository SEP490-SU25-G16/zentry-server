using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.DeviceManagement.Application.Commands;
using Zentry.Modules.DeviceManagement.Presentation.Requests;

namespace Zentry.Modules.DeviceManagement.Presentation.Controllers;

[ApiController]
[Route("api/devices")]
public class DevicesController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDeviceRequest request)
    {
        var command = new RegisterDeviceCommand(
            request.AccountId,
            request.DeviceName,
            Guid.NewGuid().ToString()
        );
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}