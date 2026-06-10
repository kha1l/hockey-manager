using System;
using System.IO;
using UnityEngine;

public static class SaveLoadService
{
    private const string SaveFileName = "save.json";

    public static string SavePath
    {
        get { return Path.Combine(Application.persistentDataPath, SaveFileName); }
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

        try
        {
            gameState.LastSavedUtc = DateTime.UtcNow.ToString("o");
            string json = JsonUtility.ToJson(gameState, true);
            File.WriteAllText(SavePath, json);

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

        try
        {
            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<GameState>(json);
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
}
