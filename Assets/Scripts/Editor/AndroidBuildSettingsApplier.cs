#if UNITY_EDITOR
#pragma warning disable 0618
using UnityEditor;
using UnityEngine;

public static class AndroidBuildSettingsApplier
{
    [MenuItem("Tools/Continental Hockey Manager/Apply Android Build Settings")]
    public static void ApplyAndroidBuildSettings()
    {
        PlayerSettings.companyName = AndroidBuildConfig.CompanyName;
        PlayerSettings.productName = AndroidBuildConfig.ProductName;
        PlayerSettings.bundleVersion = AndroidBuildConfig.BundleVersion;
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.offlinesportsmanager.continentalhockeymanager");
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;

        TryApply("Android bundle version code", () => PlayerSettings.Android.bundleVersionCode = AndroidBuildConfig.BundleVersionCode);
        TryApply("Android ARM64 architecture", () => PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64);
        TryApply("Android IL2CPP scripting backend", () => PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP));
        TryApply("Android min SDK", () => PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25);
        TryApply("Android target SDK", () => PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel35);

        Debug.Log("Applied Android build settings for " + AndroidBuildConfig.BuildVersionLabel());
    }

    private static void TryApply(string label, System.Action action)
    {
        try
        {
            action?.Invoke();
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning("Could not apply " + label + ": " + exception.Message);
        }
    }
}
#pragma warning restore 0618
#endif
