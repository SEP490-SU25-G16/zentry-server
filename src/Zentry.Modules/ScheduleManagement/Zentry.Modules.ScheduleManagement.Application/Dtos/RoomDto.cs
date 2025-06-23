namespace Zentry.Modules.ScheduleManagement.Application.Dtos;

public class RoomDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Building { get; set; }
    public int Capacity { get; set; }
}