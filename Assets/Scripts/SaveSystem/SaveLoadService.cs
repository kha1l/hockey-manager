using System;
using System.IO;
using UnityEngine;

public static class SaveLoadService
{
    private const string SaveFileName = "save.json";
    private const string BackupFileName = "save.bak";

    public static string SavePath
    {
        get { return Path.Combine(Application.persistentDataPath, SaveFileName); }
    }

    public static string BackupPath
    {
        get { return Path.Combine(Application.persistentDataPath, BackupFileName); }
    }

    public static bool SaveExists()
    {
        return File.Exists(SavePath);
    }

    public static void Save(GameState gameState)
    {
        if (gameState == null)
        {
            Debug.LogWarning("SaveLoadService: gameState is null, сохранение отменено.");
            return;
        }

        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            gameState.SaveVersion = SaveMigrationConfig.CurrentSaveVersion;
            gameState.LastSavedUtc = DateTime.UtcNow.ToString("o");
            gameState.EnsureAndroidPerformanceData();
            CreateBackupIfPossible();
            string json = JsonUtility.ToJson(gameState, false);
            File.WriteAllText(SavePath, json);
            stopwatch.Stop();
            PerformanceTimerService.RecordSave(gameState, stopwatch.ElapsedMilliseconds);

            Debug.Log("Игра сохранена: " + SavePath);
        }
        catch (Exception exception)
        {
            Debug.LogError("Ошибка сохранения игры: " + exception.Message);
        }
    }

    public static GameState Load()
    {
        if (!SaveExists())
        {
            Debug.LogWarning("Сохранение не найдено");
            return null;
        }

        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            string json = File.ReadAllText(SavePath);
            GameState state = JsonUtility.FromJson<GameState>(json);
            if (state == null)
            {
                Debug.LogError("SaveLoadService: save.json parsed to null GameState.");
                return null;
            }

            if (!TeamIdentityService.TryEnsureCompatibleGameState(state))
            {
                Debug.LogWarning("Сохранение создано для другой лиги и несовместимо с "
                    + FictionalLeagueConfig.LeagueDisplayName
                    + ". Файл сохранения не удалён.");
                return null;
            }

            SaveMigrationService.Migrate(state);
            GameStateRepairService.RepairSafeIssues(state);
            state.EnsureAlphaBalanceReports();
            stopwatch.Stop();
            PerformanceTimerService.RecordLoad(state, stopwatch.ElapsedMilliseconds);
            return state;
        }
        catch (Exception exception)
        {
            Debug.LogError("Ошибка загрузки сохранения: " + exception.Message);
            return null;
        }
    }

    public static void DeleteSave()
    {
        if (SaveExists())
        {
            File.Delete(SavePath);
            Debug.Log("Сохранение удалено: " + SavePath);
            return;
        }

        Debug.Log("Сохранение для удаления не найдено");
    }

    private static void CreateBackupIfPossible()
    {
        if (!SaveExists())
        {
            return;
        }

        try
        {
            File.Copy(SavePath, BackupPath, true);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Не удалось создать backup сохранения: " + exception.Message);
        }
    }
}
