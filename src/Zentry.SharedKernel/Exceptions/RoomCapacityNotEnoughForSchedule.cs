namespace Zentry.SharedKernel.Exceptions;

/// <summary>
///     Represents an exception thrown when a room is not available at a specific time.
/// </summary>
public class RoomCapacityNotEnoughForSchedule : BusinessLogicException
{
    public RoomCapacityNotEnoughForSchedule(string roomId, string scheduleId, string classSectionId) : base(
        $"Capacity of room '{roomId}' with schedule {scheduleId} is not enough for list student added to class section {classSectionId}.")
    {
    }

    public RoomCapacityNotEnoughForSchedule(string message) : base(message)
    {
    }

    public RoomCapacityNotEnoughForSchedule(string message, Exception innerException) : base(message, innerException)
    {
    }
}
