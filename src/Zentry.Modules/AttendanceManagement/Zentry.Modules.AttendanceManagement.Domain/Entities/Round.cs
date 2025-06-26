using Zentry.SharedKernel.Domain;

namespace Zentry.Modules.AttendanceManagement.Domain.Entities;

public class Round : AggregateRoot<Guid>
{
    private Round() : base(Guid.Empty) { }
    private Round(Guid id, Guid sessionId, Guid deviceId, DateTime startTime, DateTime endTime, string? clientRequest)
        : base(id)
    {
        SessionId = sessionId;
        DeviceId = deviceId;
        StartTime = startTime;
        EndTime = endTime;
        ClientRequest = clientRequest;
        CreatedAt = DateTime.UtcNow;
    }
    public Guid SessionId { get; private set; }
    public Guid DeviceId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public string? ClientRequest { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Round Create(Guid sessionId, Guid deviceId, DateTime startTime, DateTime endTime, string? clientRequest)
    {
        return new Round(Guid.NewGuid(), sessionId, deviceId, startTime, endTime, clientRequest);
    }

    public void Update(DateTime? startTime = null, DateTime? endTime = null, string? clientRequest = null)
    {
        if (startTime.HasValue) StartTime = startTime.Value;
        if (endTime.HasValue) EndTime = endTime.Value;
        if (!string.IsNullOrWhiteSpace(clientRequest)) ClientRequest = clientRequest;
    }
}
