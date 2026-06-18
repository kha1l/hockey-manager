public static class AndroidBuildChecklistService
{
    public static string BuildManualChecklist()
    {
        return "Android alpha checklist"
            + "\n- Apply Android Build Settings from Tools/Continental Hockey Manager."
            + "\n- Switch build target to Android in Build Settings."
            + "\n- Confirm portrait orientation and CanvasScaler 1080x1920."
            + "\n- Run Android Readiness diagnostics."
            + "\n- Start a new game, save, load, and simulate one day."
            + "\n- Check heavy lists for display limits."
            + "\n- Build a local APK/AAB and smoke test on device.";
    }
}
