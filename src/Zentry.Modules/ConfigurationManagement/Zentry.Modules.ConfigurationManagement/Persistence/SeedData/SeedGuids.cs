namespace Zentry.Modules.ConfigurationManagement.Persistence.SeedData;

public static class SeedGuids
{
    // AttributeDefinition Guids
    public static readonly Guid AttendanceWindowMinutesAttrId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");
    public static readonly Guid TotalAttendanceRoundsAttrId = Guid.Parse("B2C3D4E5-F6A7-8901-2345-67890ABCDEFA");
    public static readonly Guid AbsentReportGracePeriodHoursAttrId = Guid.Parse("C3D4E5F6-A7B8-9012-3456-7890ABCDEFAB");

    public static readonly Guid ManualAdjustmentGracePeriodHoursAttrId =
        Guid.Parse("D4E5F6A7-B8C9-0123-4567-890ABCDEFABC");
}