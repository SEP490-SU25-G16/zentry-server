using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.ConfigurationManagement.Dtos;
using Zentry.Modules.ConfigurationManagement.Features.CreateAttributeDefinition; // Namespace mới
using Zentry.Modules.ConfigurationManagement.Features.CreateSetting;
using Zentry.Modules.ConfigurationManagement.Features.GetSettings;
using Zentry.SharedKernel.Abstractions.Models;
using Zentry.SharedKernel.Constants.Response;
using Zentry.SharedKernel.Extensions;
using Zentry.SharedKernel.Helpers;
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
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetSettingsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSettings(
        [FromQuery] GetSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidationHelper.ValidateBasic(request);
        if (validationError != null) return validationError;

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
        return HandleResult(result);
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
