using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.User;

namespace Zentry.SharedKernel.Contracts.Configuration;

public record CreateUserAttributesIntegrationCommand(
    Guid UserId,
    Dictionary<string, string> UserAttributes
) : ICommand<CreateUserAttributesIntegrationResponse>;

public record CreateUserAttributesIntegrationResponse(
    bool Success,
    string Message
);
