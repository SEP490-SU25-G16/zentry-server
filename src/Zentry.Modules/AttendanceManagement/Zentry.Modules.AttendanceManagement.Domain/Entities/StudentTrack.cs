using Zentry.Modules.AttendanceManagement.Domain.ValueObjects;

namespace Zentry.Modules.AttendanceManagement.Domain.Entities;

public class StudentTrack
{
    public StudentTrack()
    {
    }

    public StudentTrack(Guid sesionId, Guid studentId, string deviceId)
    {
        SessionId = sesionId;
        Id = studentId;
        DeviceId = deviceId;
    }

    public Guid Id { get; set; }
    public string DeviceId { get; set; }
    public Guid SessionId { get; set; }
    public List<RoundParticipation> Rounds { get; set; } = [];
    public double CurrentPercentageAttended { get; private set; }

    public void SetCurrentPercentageAttended(int totalRoundsInSession)
    {
        CurrentPercentageAttended = CalculatePercentageAttended(totalRoundsInSession);
    }

    public double CalculatePercentageAttended(int totalRoundsInSession)
    {
        if (totalRoundsInSession == 0) return 0;

        // Đếm số round mà sinh viên này đã tham gia
        var attendedRoundsCount = Rounds.Count(rp => rp.IsAttended);

        return (double)attendedRoundsCount / totalRoundsInSession * 100;
    }
}