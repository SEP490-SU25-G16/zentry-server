namespace Zentry.Modules.ConfigurationManagement.Persistence.SeedData;

public static class SeedGuids
{
    // AttributeDefinition Guids
    public static readonly Guid AttendanceWindowMinutesAttrId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");
    public static readonly Guid TotalAttendanceRoundsAttrId = Guid.Parse("B2C3D4E5-F6A7-8901-2345-67890ABCDEFA");
    public static readonly Guid AbsentReportGracePeriodHoursAttrId = Guid.Parse("C3D4E5F6-A7B8-9012-3456-7890ABCDEFAB");

    public static readonly Guid ManualAdjustmentGracePeriodHoursAttrId =
        Guid.Parse("D4E5F6A7-B8C9-0123-4567-890ABCDEFABC");

    public static readonly Guid MinRssiThresholdAttrId = Guid.Parse("E5F6A7B8-C9D0-1234-5678-90ABCDEFABCD");
    public static readonly Guid MaxHopDistanceAttrId = Guid.Parse("F6A7B8C9-D0E1-2345-6789-0ABCDEFABCDE");

    // Sample Scope Guids (for Course and Session, you'd use real ones)
    public static readonly Guid SampleCourseId = Guid.Parse("12345678-ABCD-EF01-2345-67890ABCDEF0");
    public static readonly Guid SampleScheduleId = Guid.Parse("98765432-FEDC-BA09-8765-43210FEDCBA9");
}