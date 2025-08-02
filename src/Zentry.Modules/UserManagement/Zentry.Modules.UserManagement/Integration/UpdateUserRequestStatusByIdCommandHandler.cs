using Microsoft.Extensions.Logging;
using Zentry.Modules.UserManagement.Interfaces;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Attendance;
using Zentry.SharedKernel.Contracts.User;
using Zentry.SharedKernel.Exceptions;

namespace Zentry.Modules.UserManagement.Integration;

// Cập nhật record để nhận IsAccepted
public record UpdateUserRequestStatusIntegrationCommand(Guid UserRequestId, bool IsAccepted) : ICommand<UpdateUserRequestStatusIntegrationResponse>;

// Giữ nguyên Response

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
        // ... (phần kiểm tra ban đầu giữ nguyên) ...
        if (userRequest is null)
        {
            logger.LogWarning("UserRequest with ID {UserRequestId} not found.", command.UserRequestId);
            throw new NotFoundException("UserRequest", $"Yêu cầu người dùng với ID '{command.UserRequestId}' không tìm thấy.");
        }

        if (!userRequest.RequestType.Equals(RequestType.UpdateDevice))
        {
            throw new BusinessRuleException("INVALID_REQUEST_TYPE", $"Loại yêu cầu không hợp lệ. Chỉ chấp nhận yêu cầu '{RequestType.UpdateDevice.ToString()}'.");
        }

        if (!userRequest.Status.Equals(RequestStatus.Pending))
        {
            throw new BusinessRuleException("INVALID_REQUEST_STATUS", $"Yêu cầu người dùng với ID '{command.UserRequestId}' không ở trạng thái chờ duyệt.");
        }

        if (command.IsAccepted)
        {
            userRequest.Approve();
        }
        else
        {
            userRequest.Reject();
        }

        await userRequestRepository.UpdateAsync(userRequest, cancellationToken);
        await userRequestRepository.SaveChangesAsync(cancellationToken);

        return new UpdateUserRequestStatusIntegrationResponse(userRequest.RequestedByUserId, userRequest.RelatedEntityId);
    }
}
