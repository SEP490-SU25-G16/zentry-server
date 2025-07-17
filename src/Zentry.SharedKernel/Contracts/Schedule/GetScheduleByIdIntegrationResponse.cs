namespace Zentry.SharedKernel.Contracts.Schedule;

public class GetScheduleByIdIntegrationResponse
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public Guid RoomId { get; set; }
    public Guid LecturerId { get; set; }
    public DateTime ScheduledStartTime { get; set; }
    public DateTime ScheduledEndTime { get; set; }
    public bool IsActive { get; set; }
}