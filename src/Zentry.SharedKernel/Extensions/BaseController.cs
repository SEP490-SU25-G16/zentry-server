using Microsoft.AspNetCore.Mvc;
using Zentry.SharedKernel.Abstractions.Models;
using Zentry.SharedKernel.Constants.Response;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.SharedKernel.Extensions;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected IActionResult HandleResult<T>(T data, string? message = null)
    {
        return Ok(ApiResponse<T>.SuccessResult(data, message));
    }

    protected IActionResult HandleResult(string? message = null)
    {
        return Ok(ApiResponse.SuccessResult(message));
    }

    protected IActionResult HandleCreated<T>(T data, string actionName, object? routeValues = null)
    {
        var response = ApiResponse<T>.SuccessResult(data, "Resource created successfully");
        return CreatedAtAction(actionName, routeValues, response);
    }

    protected IActionResult HandleNoContent()
    {
        return NoContent();
    }

    protected IActionResult HandleError(Exception ex)
    {
        return ex switch
        {
            // User management specific exceptions (most specific first)
            UserNotFoundException => NotFound(ApiResponse.ErrorResult(ErrorCodes.UserNotFound, ex.Message)),
            UserAlreadyExistsException => Conflict(ApiResponse.ErrorResult(ErrorCodes.UserAlreadyExists, ex.Message)),
            AccountNotFoundException => NotFound(ApiResponse.ErrorResult(ErrorCodes.AccountNotFound, ex.Message)),

            // Resource exceptions
            ResourceNotFoundException => NotFound(ApiResponse.ErrorResult(ErrorCodes.ResourceNotFound, ex.Message)),
            ResourceAlreadyExistsException => Conflict(ApiResponse.ErrorResult(ErrorCodes.ResourceAlreadyExists,
                ex.Message)),

            // Schedule management exceptions
            ScheduleConflictException => Conflict(ApiResponse.ErrorResult(ErrorCodes.ScheduleConflict, ex.Message)),
            ClassSectionNotFoundException => NotFound(ApiResponse.ErrorResult(ErrorCodes.ClassSectionNotFound,
                ex.Message)),
            RoomNotAvailableException => Conflict(ApiResponse.ErrorResult(ErrorCodes.RoomNotAvailable, ex.Message)),

            // Device management exceptions
            DeviceAlreadyRegisteredException => Conflict(ApiResponse.ErrorResult(ErrorCodes.DeviceAlreadyRegistered,
                ex.Message)),

            // Attendance exceptions
            SessionNotFoundException => NotFound(ApiResponse.ErrorResult(ErrorCodes.SessionNotFound, ex.Message)),
            SessionAlreadyStartedException => Conflict(ApiResponse.ErrorResult(ErrorCodes.SessionAlreadyStarted,
                ex.Message)),
            AttendanceCalculationFailedException => BadRequest(
                ApiResponse.ErrorResult(ErrorCodes.AttendanceCalculationFailed, ex.Message)),

            // Configuration exceptions
            ConfigurationException => StatusCode(500,
                ApiResponse.ErrorResult(ErrorCodes.ConfigurationError, ex.Message)),
            SettingNotFoundException => NotFound(ApiResponse.ErrorResult(ErrorCodes.SettingNotFound, ex.Message)),

            // General business logic exceptions (after specific ones)
            BusinessLogicException ble => BadRequest(
                ApiResponse.ErrorResult(ErrorCodes.BusinessLogicError, ble.Message)),

            // Standard .NET exceptions
            InvalidOperationException ioe => BadRequest(ApiResponse.ErrorResult(ErrorCodes.InvalidOperation,
                ioe.Message)),
            ArgumentException ae => BadRequest(ApiResponse.ErrorResult(ErrorCodes.ValidationError, ae.Message)),
            DirectoryNotFoundException dnfe => NotFound(ApiResponse.ErrorResult(ErrorCodes.ResourceNotFound,
                dnfe.Message)),
            FileNotFoundException fnfe => NotFound(ApiResponse.ErrorResult(ErrorCodes.ResourceNotFound, fnfe.Message)),
            UnauthorizedAccessException => StatusCode(401,
                ApiResponse.ErrorResult(ErrorCodes.Unauthorized, "Access denied")),

            // Default case
            _ => StatusCode(500,
                ApiResponse.ErrorResult(ErrorCodes.InternalServerError, "An unexpected error occurred"))
        };
    }

    protected IActionResult HandleValidationError(string? message = null)
    {
        return BadRequest(ApiResponse.ErrorResult(ErrorCodes.ValidationError,
            message ?? "Invalid request data"));
    }

    protected IActionResult HandleNotFound(string resourceName, object id)
    {
        return NotFound(ApiResponse.ErrorResult(ErrorCodes.ResourceNotFound,
            $"{resourceName} with ID '{id}' not found"));
    }

    protected IActionResult HandleUserNotFound(object id)
    {
        return NotFound(ApiResponse.ErrorResult(ErrorCodes.UserNotFound,
            $"User with ID '{id}' not found"));
    }

    protected IActionResult HandleConflict(string errorCode, string message)
    {
        return Conflict(ApiResponse.ErrorResult(errorCode, message));
    }

    protected IActionResult HandleBadRequest(string errorCode, string message)
    {
        return BadRequest(ApiResponse.ErrorResult(errorCode, message));
    }
}
