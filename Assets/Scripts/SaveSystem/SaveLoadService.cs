using System;
using System.Collections.Generic;
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
        Save(gameState, true);
    }

    public static void SaveFast(GameState gameState)
    {
        Save(gameState, false);
    }

    public static void Save(GameState gameState, bool createBackup)
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
            if (createBackup)
            {
                CreateBackupIfPossible();
            }

            SaveCompactionScope compactionScope = SaveCompactionScope.Apply(gameState);
            try
            {
                string json = JsonUtility.ToJson(gameState, false);
                File.WriteAllText(SavePath, json);
            }
            finally
            {
                compactionScope.Restore();
            }

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

    private sealed class SaveCompactionScope
    {
        private readonly List<CompactedResult> _compactedResults = new List<CompactedResult>();

        public static SaveCompactionScope Apply(GameState state)
        {
            SaveCompactionScope scope = new SaveCompactionScope();
            scope.Compact(state);
            return scope;
        }

        public void Restore()
        {
            for (int i = 0; i < _compactedResults.Count; i++)
            {
                CompactedResult compacted = _compactedResults[i];
                if (compacted.Result == null)
                {
                    continue;
                }

                compacted.Result.PlayerStats = compacted.PlayerStats;
                compacted.Result.Events = compacted.Events;
            }

            _compactedResults.Clear();
        }

        private void Compact(GameState state)
        {
            if (state == null)
            {
                return;
            }

            HashSet<MatchResultData> seenResults = new HashSet<MatchResultData>();
            CompactResult(state.LastMatchResult, seenResults);
            CompactResults(state.MatchHistory, seenResults);

            SeasonData season = state.Season;
            if (season == null)
            {
                return;
            }

            if (season.Schedule != null)
            {
                foreach (ScheduleGameData game in season.Schedule)
                {
                    if (game != null)
                    {
                        CompactResult(game.Result, seenResults);
                    }
                }
            }

            PlayoffData playoffs = season.Playoffs;
            if (playoffs == null || playoffs.Rounds == null)
            {
                return;
            }

            foreach (PlayoffRoundData round in playoffs.Rounds)
            {
                if (round == null || round.Series == null)
                {
                    continue;
                }

                foreach (PlayoffSeriesData series in round.Series)
                {
                    if (series != null)
                    {
                        CompactResults(series.Games, seenResults);
                    }
                }
            }
        }

        private void CompactResults(List<MatchResultData> results, HashSet<MatchResultData> seenResults)
        {
            if (results == null)
            {
                return;
            }

            foreach (MatchResultData result in results)
            {
                CompactResult(result, seenResults);
            }
        }

        private void CompactResult(MatchResultData result, HashSet<MatchResultData> seenResults)
        {
            if (result == null || seenResults == null || !seenResults.Add(result))
            {
                return;
            }

            List<PlayerGameStatData> playerStats = result.PlayerStats;
            List<LiveMatchEventData> events = result.Events;
            _compactedResults.Add(new CompactedResult
            {
                Result = result,
                PlayerStats = playerStats,
                Events = events
            });

            result.PlayerStats = new List<PlayerGameStatData>();
            result.Events = new List<LiveMatchEventData>();
        }

        private struct CompactedResult
        {
            public MatchResultData Result;
            public List<PlayerGameStatData> PlayerStats;
            public List<LiveMatchEventData> Events;
        }
    }
}
