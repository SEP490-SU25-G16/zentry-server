using Zentry.Modules.AttendanceManagement.Domain.ValueObjects;

namespace Zentry.Modules.AttendanceManagement.Domain.Entities;
public class StudentTrack
{
    public Guid Id { get; set; }
    public string DeviceId { get; set; }

    public List<RoundParticipation> Rounds { get; set; } = [];

    public StudentTrack() {}

    public StudentTrack(Guid studentId, string deviceId)
    {
        Id = studentId;
        DeviceId = deviceId;
    }
}

