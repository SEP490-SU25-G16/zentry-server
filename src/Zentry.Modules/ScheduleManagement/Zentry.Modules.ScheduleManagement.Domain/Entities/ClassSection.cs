using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.ScheduleManagement.Domain.Entities;

public class ClassSection : AggregateRoot<Guid>
{
    private ClassSection() : base(Guid.Empty)
    {
        Schedules = new List<Schedule>();
        Enrollments = new List<Enrollment>();
    }

    private ClassSection(Guid id, Guid courseId, string sectionCode, string semester)
        : base(id)
    {
        CourseId = courseId;
        SectionCode = sectionCode;
        Semester = semester;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsDeleted = false;

        Schedules = new HashSet<Schedule>();
        Enrollments = new HashSet<Enrollment>();
    }

    public Guid CourseId { get; private set; }
    public virtual Course? Course { get; private set; }
    public Guid? LecturerId { get; private set; }
    public string SectionCode { get; private set; }
    public string Semester { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public virtual ICollection<Schedule> Schedules { get; private set; }

    public virtual ICollection<Enrollment> Enrollments { get; private set; }
    // --------------------------------------------------------------------------------

    public static ClassSection Create(Guid courseId, string sectionCode, string semester)
    {
        return new ClassSection(Guid.NewGuid(), courseId, sectionCode, semester);
    }

    public void Update(string? sectionCode = null, string? semester = null)
    {
        if (!string.IsNullOrWhiteSpace(sectionCode)) SectionCode = sectionCode;
        if (!string.IsNullOrWhiteSpace(semester)) Semester = semester;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
    public void AssignLecturer(Guid lecturerId)
    {
        if (LecturerId == lecturerId) return;
        LecturerId = lecturerId;
        UpdatedAt = DateTime.UtcNow;
    }
}
