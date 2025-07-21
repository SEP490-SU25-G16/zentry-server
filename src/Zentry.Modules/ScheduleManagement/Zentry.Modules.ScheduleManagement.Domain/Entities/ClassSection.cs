using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.ScheduleManagement.Domain.Entities;

public class ClassSection : AggregateRoot<Guid>
{
    private ClassSection() : base(Guid.Empty) {}

    private ClassSection(Guid id, Guid courseId, Guid lecturerId, string sectionCode, string semester)
        : base(id)
    {
        CourseId = courseId;
        LecturerId = lecturerId;
        SectionCode = sectionCode;
        Semester = semester;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid CourseId { get; private set; }
    public Guid LecturerId { get; private set; }
    public string SectionCode { get; private set; }
    public string Semester { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual Course? Course { get; private set; }

    public static ClassSection Create(Guid courseId, Guid lecturerId, string sectionCode, string semester)
    {
        return new ClassSection(Guid.NewGuid(), courseId, lecturerId, sectionCode, semester);
    }

    public void Update(string? sectionCode = null, string? semester = null)
    {
        if (!string.IsNullOrWhiteSpace(sectionCode)) SectionCode = sectionCode;
        if (!string.IsNullOrWhiteSpace(semester)) Semester = semester;
        UpdatedAt = DateTime.UtcNow;
    }
}
