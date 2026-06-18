public static class AndroidBuildConfig
{
    public const string ProductName = "Continental Hockey Manager";
    public const string CompanyName = "Offline Sports Manager";
    public const string BundleVersion = "0.1.0-alpha";
    public const int BundleVersionCode = 1;
    public const int TargetFrameRate = 60;
    public const int PortraitReferenceWidth = 1080;
    public const int PortraitReferenceHeight = 1920;
    public const int MinSdkVersionFallback = 25;
    public const int TargetSdkVersionFallback = 35;
    public const bool UsePortraitOrientation = true;

    public static string BuildVersionLabel()
    {
        return BundleVersion + " (" + BundleVersionCode + ")";
    }
}
