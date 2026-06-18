using UnityEngine;
using UnityEngine.UI;

public class NewsItemRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;
    [SerializeField] private Button _button;

    private string _newsId = "";
    private GameScreenController _screenController;

    public void Configure(Text infoText, Button button)
    {
        _infoText = infoText;
        _button = button;
    }

    public void Initialize(NewsItemData item, GameScreenController screenController)
    {
        _screenController = screenController;
        _newsId = item == null ? "" : item.NewsId;

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(MarkAsRead);
        }

        if (_infoText == null)
        {
            return;
        }

        if (item == null)
        {
            _infoText.text = "Новость недоступна";
            return;
        }

        string unread = item.IsRead ? "Read" : "New";
        string team = string.IsNullOrEmpty(item.TeamName) ? "" : " | " + item.TeamName;
        string player = string.IsNullOrEmpty(item.PlayerName) ? "" : " | " + item.PlayerName;

        _infoText.text = SafeText(item.Title, "News update")
            + " | " + unread
            + " | " + SafeText(item.DateLabel, "no date")
            + " | " + SafeText(item.Category, "News")
            + " | " + NewsConfig.GetImportanceLabel(item.Importance)
            + team
            + player
            + "\n" + SafeText(item.Body, "");
    }

    private void MarkAsRead()
    {
        if (_screenController != null && !string.IsNullOrEmpty(_newsId))
        {
            _screenController.MarkNewsAsRead(_newsId);
        }
    }

    private static string SafeText(string value, string fallback)
    {
        return string.IsNullOrEmpty(value) ? fallback : value;
    }
}
