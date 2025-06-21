namespace Zentry.Modules.Configuration.Application.Dtos;

public record ConfigurationDto(
    Guid Id,
    string Key,
    string Value,
    string? Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt);