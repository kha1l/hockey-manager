using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void StartNewGame()
    {
        GameSession.Clear();
        Debug.Log("Переход к выбору команды");
        SceneManager.LoadScene("TeamSelect");
    }

    public void LoadGame()
    {
        if (!SaveLoadService.SaveExists())
        {
            Debug.LogWarning("Сохранение не найдено");
            return;
        }

        GameState gameState = SaveLoadService.Load();
        GameSession.LoadGame(gameState);

        if (gameState != null)
        {
            SceneManager.LoadScene("Game");
        }
    }

    public void OpenSettings()
    {
        Debug.Log("Настройки пока не реализованы");
    }

    public void ExitGame()
    {
        Debug.Log("Выход из игры");
        Application.Quit();
    }
}
