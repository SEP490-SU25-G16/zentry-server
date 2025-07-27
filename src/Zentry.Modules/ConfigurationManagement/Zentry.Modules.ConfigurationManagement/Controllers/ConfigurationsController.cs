using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.ConfigurationManagement.Dtos;
using Zentry.Modules.ConfigurationManagement.Features.CreateSetting;
using Zentry.Modules.ConfigurationManagement.Features.GetSettings;
using Zentry.SharedKernel.Abstractions.Models;
using Zentry.SharedKernel.Constants.Response;
using Zentry.SharedKernel.Extensions;

namespace Zentry.Modules.ConfigurationManagement.Controllers;

[ApiController]
[Route("api/configurations")]
public class ConfigurationsController(IMediator mediator) : BaseController
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetSettingsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
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
            return HandleResult(result);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateSettingResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateSetting([FromBody] CreateSettingRequest request,
        CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest(ApiResponse.ErrorResult(ErrorCodes.ValidationError, "Request body is required."));

        if (request.AttributeDefinitionDetails == null)
            return BadRequest(ApiResponse.ErrorResult(ErrorCodes.ValidationError,
                "AttributeDefinitionDetails is required."));

        if (string.IsNullOrWhiteSpace(request.AttributeDefinitionDetails.Key))
            return BadRequest(ApiResponse.ErrorResult(ErrorCodes.ValidationError,
                "AttributeDefinitionDetails.Key is required."));

        if (string.IsNullOrWhiteSpace(request.AttributeDefinitionDetails.DisplayName))
            return BadRequest(ApiResponse.ErrorResult(ErrorCodes.ValidationError,
                "AttributeDefinitionDetails.DisplayName is required."));

        if (string.IsNullOrWhiteSpace(request.AttributeDefinitionDetails.DataType))
            return BadRequest(ApiResponse.ErrorResult(ErrorCodes.ValidationError,
                "AttributeDefinitionDetails.DataType is required."));

        if (string.IsNullOrWhiteSpace(request.AttributeDefinitionDetails.ScopeType))
            return BadRequest(ApiResponse.ErrorResult(ErrorCodes.ValidationError,
                "AttributeDefinitionDetails.ScopeType is required."));

        if (request.Setting == null)
            return BadRequest(ApiResponse.ErrorResult(ErrorCodes.ValidationError, "Setting details are required."));

        if (string.IsNullOrWhiteSpace(request.Setting.ScopeType))
            return BadRequest(ApiResponse.ErrorResult(ErrorCodes.ValidationError, "Setting.ScopeType is required."));

        if (request.Setting.ScopeId == Guid.Empty)
            return BadRequest(ApiResponse.ErrorResult(ErrorCodes.ValidationError, "Setting.ScopeId is required."));

        if (string.IsNullOrWhiteSpace(request.Setting.Value))
            return BadRequest(ApiResponse.ErrorResult(ErrorCodes.ValidationError, "Setting.Value is required."));

        try
        {
            var command = new CreateSettingCommand
            {
                AttributeDefinitionDetails = request.AttributeDefinitionDetails,
                Setting = request.Setting
            };

            var response = await mediator.Send(command, cancellationToken);
            return HandleCreated(response, nameof(GetSettings), new { id = response.SettingId });
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }
}