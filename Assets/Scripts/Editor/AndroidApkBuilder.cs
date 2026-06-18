#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class AndroidApkBuilder
{
    private const string OutputFileName = "hockey-manager.apk";

    [MenuItem("Tools/Continental Hockey Manager/Build Android APK")]
    public static void BuildAndroidApk()
    {
        string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", OutputFileName));
        BuildAndroidApk(outputPath);
    }

    public static void BuildAndroidApkFromCommandLine()
    {
        BuildAndroidApk();
    }

    private static void BuildAndroidApk(string outputPath)
    {
        InitialSceneCreator.CreateInitialScenes();
        AndroidBuildSettingsApplier.ApplyAndroidBuildSettings();

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[]
            {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/TeamSelect.unity",
                "Assets/Scenes/Game.unity"
            },
            locationPathName = outputPath,
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;
        if (summary.result != BuildResult.Succeeded)
        {
            throw new System.Exception("Android APK build failed: " + summary.result);
        }

        Debug.Log("Android APK created: " + outputPath + " (" + summary.totalSize + " bytes)");
    }
}
#endif
