using UnityEngine;
using UnityEngine.UI;

public class MobilePanelHeaderView : MonoBehaviour
{
    public Text TitleText;
    public Text SubtitleText;
    public Button BackButton;

    public void Initialize(string title, string subtitle, GameScreenController screenController)
    {
        if (TitleText != null)
        {
            TitleText.text = string.IsNullOrEmpty(title) ? "" : title;
        }

        if (SubtitleText != null)
        {
            SubtitleText.text = string.IsNullOrEmpty(subtitle) ? "" : subtitle;
        }

        if (BackButton != null)
        {
            BackButton.onClick.RemoveAllListeners();
            BackButton.onClick.AddListener(delegate
            {
                if (screenController != null)
                {
                    screenController.ShowDashboard();
                }
            });
        }
    }
}
