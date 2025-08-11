using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.SharedKernel.Contracts.Configuration;

public record GetUserAttributesForUsersIntegrationQuery(List<Guid> UserIds)
    : IQuery<GetUserAttributesForUsersIntegrationResponse>;

// Response: Dictionary<UserId, Dictionary<AttributeKey, AttributeValue>>
public record GetUserAttributesForUsersIntegrationResponse(
    Dictionary<Guid, Dictionary<string, string>> UserAttributes
);
