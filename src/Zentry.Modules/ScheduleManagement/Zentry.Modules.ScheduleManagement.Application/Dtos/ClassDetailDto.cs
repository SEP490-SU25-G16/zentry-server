using System;

namespace Zentry.Modules.ScheduleManagement.Application.Dtos;

public class ClassDetailDto
{
    public string CourseName { get; set; } = string.Empty;

    public int EnrolledStudentsCount { get; set; }

    public int DurationInMinutes { get; set; } // Ví dụ: 90 phút

    public string Building { get; set; } = string.Empty;

    public Guid ClassSectionId { get; set; }
}
