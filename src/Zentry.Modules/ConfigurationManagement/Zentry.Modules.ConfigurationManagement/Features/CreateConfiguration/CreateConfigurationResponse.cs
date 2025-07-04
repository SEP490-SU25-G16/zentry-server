﻿using Zentry.Modules.ConfigurationManagement.Dtos;
using Zentry.Modules.ConfigurationManagement.Persistence.Enums;

namespace Zentry.Modules.ConfigurationManagement.Features.CreateConfiguration;

public class CreateConfigurationResponse
{
    public Guid ConfigurationId { get; set; }

    // Thông tin của AttributeDefinition đã được tạo/cập nhật
    public Guid AttributeId { get; set; }
    public string AttributeKey { get; set; } = string.Empty;
    public string AttributeDisplayName { get; set; } = string.Empty;
    public DataType DataType { get; set; }
    public ScopeType AttributeDefinitionScopeType { get; set; } // ScopeType từ AttributeDefinition
    public string? Unit { get; set; }

    // Thông tin về Options (nếu có và DataType là Selection)
    public List<OptionDto>? Options { get; set; } // Cần tạo OptionDto riêng

    // Thông tin của Configuration
    public ScopeType ConfigurationScopeType { get; set; } // ScopeType từ Configuration
    public Guid ScopeId { get; set; }
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
