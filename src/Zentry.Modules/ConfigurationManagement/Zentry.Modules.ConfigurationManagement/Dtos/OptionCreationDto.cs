﻿namespace Zentry.Modules.ConfigurationManagement.Dtos;

public class OptionCreationDto
{
    public string Value { get; set; } = string.Empty;
    public string DisplayLabel { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
