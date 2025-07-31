using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zentry.Modules.UserManagement.Features.CreateUser;
using Zentry.Modules.UserManagement.Features.DeleteUser;
using Zentry.Modules.UserManagement.Features.GetUser;
using Zentry.Modules.UserManagement.Features.GetUsers;
using Zentry.Modules.UserManagement.Features.UpdateUser;
using Zentry.SharedKernel.Abstractions.Models;
using Zentry.SharedKernel.Constants.Response;
using Zentry.SharedKernel.Extensions;

namespace Zentry.Modules.UserManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IMediator mediator) : BaseController
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetUsersResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query)
    {
        try
        {
            var response = await mediator.Send(query);
            return HandleResult(response);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<GetUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserDetail(Guid id)
    {
        var query = new GetUserQuery(id);

        try
        {
            var response = await mediator.Send(query);

            if (response == null)
                return NotFound(ApiResponse.ErrorResult(ErrorCodes.UserNotFound,
                    $"User with ID '{id}' not found"));

            return HandleResult(response);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateUserResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var command = new CreateUserCommand(request);

        try
        {
            var response = await mediator.Send(command);
            return HandleCreated(response, nameof(GetUserDetail), new { id = response.UserId });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(ApiResponse.ErrorResult(ErrorCodes.UserAlreadyExists, ex.Message));
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UpdateUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        var command = new UpdateUserCommand(id, request);

        try
        {
            var response = await mediator.Send(command);

            if (!response.Success)
            {
                // Sử dụng các ErrorCodes cụ thể thay vì kiểm tra message
                if (response.Message?.Contains("User not found") == true)
                    return NotFound(ApiResponse.ErrorResult(ErrorCodes.UserNotFound, response.Message));

                if (response.Message?.Contains("Associated account not found") == true ||
                    response.Message?.Contains("Account not found") == true)
                    return NotFound(ApiResponse.ErrorResult(ErrorCodes.AccountNotFound, response.Message));

                return BadRequest(ApiResponse.ErrorResult(ErrorCodes.BusinessLogicError, response.Message));
            }

            return HandleResult(response);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<DeleteUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SoftDeleteUser(Guid id)
    {
        var command = new DeleteUserCommand(id);

        try
        {
            var response = await mediator.Send(command);

            if (!response.Success)
            {
                if (response.Message?.Contains("not found") == true)
                    return NotFound(ApiResponse.ErrorResult(ErrorCodes.UserNotFound, response.Message));

                return BadRequest(ApiResponse.ErrorResult(ErrorCodes.BusinessLogicError, response.Message));
            }

            return HandleResult(response);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }
}