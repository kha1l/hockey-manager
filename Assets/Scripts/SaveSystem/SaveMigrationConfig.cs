public static class SaveMigrationConfig
{
    public const int CurrentSaveVersion = 23;

    public const string MigrationStatusNotNeeded = "NotNeeded";
    public const string MigrationStatusMigrated = "Migrated";
    public const string MigrationStatusFailed = "Failed";

    public static bool RequiresMigration(int saveVersion)
    {
        return saveVersion <= 0 || saveVersion < CurrentSaveVersion;
    }
}
