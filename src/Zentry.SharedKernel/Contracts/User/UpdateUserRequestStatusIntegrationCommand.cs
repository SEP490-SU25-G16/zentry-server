using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.SharedKernel.Contracts.User;

public record UpdateUserRequestStatusIntegrationCommand(Guid UserRequestId) : ICommand<UpdateUserRequestStatusIntegrationResponse>;

public record UpdateUserRequestStatusIntegrationResponse(Guid UserId, Guid RelatedEntityId);
