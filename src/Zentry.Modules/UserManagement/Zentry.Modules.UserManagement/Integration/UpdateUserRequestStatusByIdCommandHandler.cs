using Microsoft.Extensions.Logging;
using Zentry.Modules.UserManagement.Interfaces;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Attendance;
using Zentry.SharedKernel.Contracts.User;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.UserManagement.Integration;

public class UpdateUserRequestStatusByIdCommandHandler(
    IUserRequestRepository userRequestRepository,
    ILogger<UpdateUserRequestStatusByIdCommandHandler> logger)
    : ICommandHandler<UpdateUserRequestStatusIntegrationCommand,
        UpdateUserRequestStatusIntegrationResponse>
{
    public async Task<UpdateUserRequestStatusIntegrationResponse> Handle(
        UpdateUserRequestStatusIntegrationCommand command,
        CancellationToken cancellationToken)
    {
        var userRequest = await userRequestRepository.GetByIdAsync(command.UserRequestId, cancellationToken);
        if (userRequest is null)
        {
            logger.LogWarning("Acceptance failed: UserRequest with ID {UserRequestId} not found.",
                command.UserRequestId);
            throw new NotFoundException("UserRequest",
                $"Yêu cầu người dùng với ID '{command.UserRequestId}' không tìm thấy.");
        }

        if (!userRequest.RequestType.Equals(RequestType.UpdateDevice))
        {
            logger.LogWarning(
                "Acceptance failed: UserRequest {UserRequestId} is of type '{RequestType}' not '{ExpectedType}'.",
                command.UserRequestId, userRequest.RequestType.ToString(), RequestType.UpdateDevice.ToString());
            throw new BusinessRuleException("INVALID_REQUEST_TYPE",
                $"Loại yêu cầu không hợp lệ. Chỉ chấp nhận yêu cầu '{RequestType.UpdateDevice.ToString()}'.");
        }

        if (!userRequest.Status.Equals(RequestStatus.Pending))
        {
            logger.LogWarning(
                "Acceptance failed: UserRequest {UserRequestId} is not in Pending status. Current status: {CurrentStatus}.",
                command.UserRequestId, userRequest.Status.ToString());
            throw new BusinessRuleException("INVALID_REQUEST_STATUS",
                $"Yêu cầu người dùng với ID '{command.UserRequestId}' không ở trạng thái chờ duyệt.");
        }

        userRequest.Approve();
        await userRequestRepository.UpdateAsync(userRequest, cancellationToken);
        await userRequestRepository.SaveChangesAsync(cancellationToken);
        return new UpdateUserRequestStatusIntegrationResponse(userRequest.RequestedByUserId,
            userRequest.RelatedEntityId);
    }
}
