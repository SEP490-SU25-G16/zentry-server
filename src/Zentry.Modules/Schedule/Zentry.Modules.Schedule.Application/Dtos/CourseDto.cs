namespace Zentry.Modules.Schedule.Application.Dtos;

public class CourseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Semester { get; set; }
    public Guid LecturerId { get; set; }
}