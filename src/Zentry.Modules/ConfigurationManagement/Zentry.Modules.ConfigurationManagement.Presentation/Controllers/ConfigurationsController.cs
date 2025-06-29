using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.ConfigurationManagement.Application.Dtos;
using Zentry.Modules.ConfigurationManagement.Application.Features.ViewConfigurations;

namespace Zentry.Modules.ConfigurationManagement.Presentation.Controllers;

[ApiController]
[Route("api/configurations")]
public class ConfigurationsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<ConfigurationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConfigurations()
    {
        var query = new GetConfigurationsQuery();
        var result = await mediator.Send(query);
        return Ok(result);
    }
}