using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.ScheduleManagement.Domain.Entities;

public class Course : AggregateRoot
{
    private Course() : base(Guid.Empty)
    {
    } // For EF Core

    public Course(Guid courseId, string code, string name, string semester, Guid lecturerId) : base(courseId)
    {
        CourseId = courseId;
        Code = !string.IsNullOrWhiteSpace(code)
            ? code
            : throw new ArgumentException("Code cannot be empty.", nameof(code));
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new ArgumentException("Name cannot be empty.", nameof(name));
        Semester = !string.IsNullOrWhiteSpace(semester)
            ? semester
            : throw new ArgumentException("Semester cannot be empty.", nameof(semester));
        LecturerId = lecturerId != Guid.Empty
            ? lecturerId
            : throw new ArgumentException("LecturerId cannot be empty.", nameof(lecturerId));
    }

    public Guid CourseId { get; private set; } = Guid.NewGuid();
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string Semester { get; private set; }
    public Guid LecturerId { get; private set; }
}