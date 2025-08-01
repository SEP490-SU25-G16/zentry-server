﻿using Zentry.Modules.ConfigurationManagement.Dtos;
using Zentry.SharedKernel.Abstractions.Application;
using Zentry.SharedKernel.Constants.Configuration;

namespace Zentry.Modules.ConfigurationManagement.Features.CreateSetting;

public class CreateSettingCommand : ICommand<CreateSettingResponse>
{
    public CreateSettingRequest SettingDetails { get; set; } = new();
}

public class CreateSettingResponse
{
    public Guid SettingId { get; set; }
    public Guid AttributeId { get; set; }
    public string AttributeKey { get; set; } = string.Empty;
    public string AttributeDisplayName { get; set; } = string.Empty;
    public DataType DataType { get; set; }
    public string? Unit { get; set; }
    public ScopeType SettingScopeType { get; set; }
    public Guid ScopeId { get; set; }
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
