using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.ScheduleManagement.Domain.Entities;

public class Course : AggregateRoot<Guid>
{
    private Course() : base(Guid.Empty)
    {
    }

    private Course(Guid id, string code, string name, string description, string semester)
        : base(id)
    {
        Code = code;
        Name = name;
        Description = description;
        Semester = semester;
        CreatedAt = DateTime.UtcNow;
    }

    public string Code { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Semester { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; private set; }

    public static Course Create(string code, string name, string description, string semester)
    {
        return new Course(Guid.NewGuid(), code, name, description, semester);
    }

    public void Update(string? name = null, string? description = null, string? semester = null)
    {
        if (!string.IsNullOrWhiteSpace(name)) Name = name;
        if (!string.IsNullOrWhiteSpace(description)) Description = description;
        if (!string.IsNullOrWhiteSpace(semester)) Semester = semester;
        UpdatedAt = DateTime.UtcNow;
    }
    public void Delete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
