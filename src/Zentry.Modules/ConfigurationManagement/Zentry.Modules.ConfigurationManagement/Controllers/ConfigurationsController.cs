using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.ConfigurationManagement.Dtos;
using Zentry.Modules.ConfigurationManagement.Features.CreateSetting;
using Zentry.Modules.ConfigurationManagement.Features.GetSettings;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.ConfigurationManagement.Controllers;

[ApiController]
[Route("api/configurations")]
public class ConfigurationsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(GetSettingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSettings([FromQuery] GetSettingsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetSettingsQuery
            {
                AttributeId = request.AttributeId,
                ScopeTypeString = request.ScopeType,
                ScopeId = request.ScopeId,
                SearchTerm = request.SearchTerm,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            var result = await mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (BusinessLogicException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An unexpected error occurred while retrieving settings." });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateSettingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateSetting([FromBody] CreateSettingRequest request,
        CancellationToken cancellationToken)
    {
        // Validate request
        if (request == null) return BadRequest(new { message = "Request body is required." });

        if (request.AttributeDefinitionDetails == null)
            return BadRequest(new { message = "AttributeDefinitionDetails is required." });

        if (string.IsNullOrWhiteSpace(request.AttributeDefinitionDetails.Key))
            return BadRequest(new { message = "AttributeDefinitionDetails.Key is required." });

        if (string.IsNullOrWhiteSpace(request.AttributeDefinitionDetails.DisplayName))
            return BadRequest(new { message = "AttributeDefinitionDetails.DisplayName is required." });

        if (string.IsNullOrWhiteSpace(request.AttributeDefinitionDetails.DataType))
            return BadRequest(new { message = "AttributeDefinitionDetails.DataType is required." });

        if (string.IsNullOrWhiteSpace(request.AttributeDefinitionDetails.ScopeType))
            return BadRequest(new { message = "AttributeDefinitionDetails.ScopeType is required." });

        if (request.Setting == null) return BadRequest(new { message = "Setting details are required." });

        if (string.IsNullOrWhiteSpace(request.Setting.ScopeType))
            return BadRequest(new { message = "Setting.ScopeType is required." });

        if (request.Setting.ScopeId == Guid.Empty)
            return BadRequest(new { message = "Setting.ScopeId is required." });

        if (string.IsNullOrWhiteSpace(request.Setting.Value))
            return BadRequest(new { message = "Setting.Value is required." });

        try
        {
            var command = new CreateSettingCommand
            {
                AttributeDefinitionDetails = request.AttributeDefinitionDetails,
                Setting = request.Setting
            };

            var response = await mediator.Send(command, cancellationToken);

            return CreatedAtAction(
                nameof(GetSettings),
                new { id = response.SettingId },
                response);
        }
        catch (BusinessLogicException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An unexpected error occurred while creating the Setting." });
        }
    }
}