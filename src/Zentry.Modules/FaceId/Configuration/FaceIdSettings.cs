namespace Zentry.Modules.FaceId.Configuration;

public class FaceIdSettings
{
    public const string SectionName = "FaceId";
    
    public float DefaultThreshold { get; set; } = 0.7f;
    public float UpdateThreshold { get; set; } = 0.7f;
    public float VerificationThreshold { get; set; } = 0.7f;
}