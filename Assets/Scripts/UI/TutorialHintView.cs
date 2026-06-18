using UnityEngine;
using UnityEngine.UI;

public class TutorialHintView : MonoBehaviour
{
    public Text TitleText;
    public Text BodyText;
    public Button DismissButton;
    public Button HelpButton;

    public void Configure(Text titleText, Text bodyText, Button dismissButton, Button helpButton)
    {
        TitleText = titleText;
        BodyText = bodyText;
        DismissButton = dismissButton;
        HelpButton = helpButton;
    }

    public void Initialize(TutorialHintData hint, GameScreenController screenController)
    {
        if (DismissButton != null)
        {
            DismissButton.onClick.RemoveAllListeners();
            DismissButton.onClick.AddListener(screenController.DismissCurrentTutorialHint);
        }

        if (HelpButton != null)
        {
            HelpButton.onClick.RemoveAllListeners();
            HelpButton.onClick.AddListener(screenController.ShowTutorial);
        }

        if (hint == null)
        {
            Clear();
            return;
        }

        gameObject.SetActive(true);
        if (TitleText != null)
        {
            TitleText.text = hint.Title;
        }

        if (BodyText != null)
        {
            BodyText.text = hint.Body;
        }
    }

    public void Clear()
    {
        if (TitleText != null)
        {
            TitleText.text = "";
        }

        if (BodyText != null)
        {
            BodyText.text = "";
        }

        gameObject.SetActive(false);
    }
}
