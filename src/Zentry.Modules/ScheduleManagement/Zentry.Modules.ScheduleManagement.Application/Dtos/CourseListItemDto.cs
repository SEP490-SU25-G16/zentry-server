﻿namespace Zentry.Modules.ScheduleManagement.Application.Dtos;

public class CourseListItemDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Semester { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
