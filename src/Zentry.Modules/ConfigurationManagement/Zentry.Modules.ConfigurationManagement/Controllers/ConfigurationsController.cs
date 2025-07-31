using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.ConfigurationManagement.Features.CreateAttributeDefinition;
using Zentry.Modules.ConfigurationManagement.Features.CreateSetting;
using Zentry.Modules.ConfigurationManagement.Features.GetListAttributeDefinition;
using Zentry.Modules.ConfigurationManagement.Features.GetSettings;
using Zentry.SharedKernel.Abstractions.Models;
using Zentry.SharedKernel.Extensions;
// Namespace mới
using CreateSettingRequest = Zentry.Modules.ConfigurationManagement.Features.CreateSetting.CreateSettingRequest;

namespace Zentry.Modules.ConfigurationManagement.Controllers;

[ApiController]
[Route("api/configurations")]
public class ConfigurationsController(
    IMediator mediator,
    IValidator<CreateAttributeDefinitionRequest> createAttributeDefinitionValidator,
    IValidator<CreateSettingRequest> createSettingValidator)
    : BaseController
{
    [HttpGet("settings")]
    [ProducesResponseType(typeof(ApiResponse<GetSettingsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSettings(
        [FromQuery] GetSettingsQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await mediator.Send(query, cancellationToken);
            return HandleResult(response);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpGet("definitions")]
    [ProducesResponseType(typeof(ApiResponse<GetListAttributeDefinitionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    // [ValidateQueryParameters] // Có thể thêm filter này nếu bạn có cho các query params
    public async Task<IActionResult> GetListAttributeDefinition(
        [FromQuery] GetListAttributeDefinitionQuery query, // Sử dụng Query DTO trực tiếp làm FromQuery
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await mediator.Send(query, cancellationToken);
            return HandleResult(response);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPost("definitions")]
    [ProducesResponseType(typeof(ApiResponse<CreateAttributeDefinitionResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAttributeDefinition(
        [FromBody] CreateAttributeDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateRequest(request, createAttributeDefinitionValidator);
        if (validationError != null) return validationError;

        var command = new CreateAttributeDefinitionCommand
        {
            Details = request
        };

        var response = await mediator.Send(command, cancellationToken);
        return HandleCreated(response, nameof(CreateAttributeDefinition), new { id = response.AttributeId });
    }


    [HttpPost("settings")]
    [ProducesResponseType(typeof(ApiResponse<CreateSettingResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSetting(
        [FromBody] CreateSettingRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateRequest(request, createSettingValidator);
        if (validationError != null) return validationError;

        var command = new CreateSettingCommand
        {
            SettingDetails = request
        };

        var response = await mediator.Send(command, cancellationToken);
        return HandleCreated(response, nameof(CreateSetting), new { id = response.SettingId });
    }
}