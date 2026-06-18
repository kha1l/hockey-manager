using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{
    public Text TitleText;
    public Text SummaryText;
    public Text ChecklistText;
    public Text HintText;
    public Button DismissHintButton;
    public Button DisableButton;
    public Button ResetButton;
    public Button CloseButton;

    public void Configure(
        Text titleText,
        Text summaryText,
        Text checklistText,
        Text hintText,
        Button dismissHintButton,
        Button disableButton,
        Button resetButton,
        Button closeButton)
    {
        TitleText = titleText;
        SummaryText = summaryText;
        ChecklistText = checklistText;
        HintText = hintText;
        DismissHintButton = dismissHintButton;
        DisableButton = disableButton;
        ResetButton = resetButton;
        CloseButton = closeButton;
    }

    public void ShowTutorial(GameState state, string currentPanelId, GameScreenController screenController)
    {
        TutorialService.EnsureTutorial(state);

        if (DismissHintButton != null)
        {
            DismissHintButton.onClick.RemoveAllListeners();
            DismissHintButton.onClick.AddListener(screenController.DismissCurrentTutorialHint);
        }

        if (DisableButton != null)
        {
            DisableButton.onClick.RemoveAllListeners();
            DisableButton.onClick.AddListener(screenController.DisableTutorial);
        }

        if (ResetButton != null)
        {
            ResetButton.onClick.RemoveAllListeners();
            ResetButton.onClick.AddListener(screenController.ResetTutorial);
        }

        if (CloseButton != null)
        {
            CloseButton.onClick.RemoveAllListeners();
            CloseButton.onClick.AddListener(screenController.HideTutorial);
        }

        if (TitleText != null)
        {
            TitleText.text = "Обучение";
        }

        if (state == null || state.Tutorial == null)
        {
            SetText(SummaryText, "Обучение недоступно");
            SetText(ChecklistText, "");
            SetText(HintText, "");
            return;
        }

        if (!state.Tutorial.IsTutorialEnabled)
        {
            SetText(SummaryText, "Обучение выключено. Нажми сброс, чтобы начать заново.");
            SetText(ChecklistText, "");
            SetText(HintText, "");
            return;
        }

        SetText(SummaryText, TutorialService.BuildTutorialSummary(state) + "\nПервые шаги помогут проверить состав, линии, таблицу, контракты и сохранение.");
        SetText(ChecklistText, BuildChecklist(state));

        TutorialHintData hint = TutorialService.GetPanelHint(state, currentPanelId);
        SetText(HintText, hint == null ? "Подсказка для этой панели скрыта или недоступна." : hint.Title + "\n" + hint.Body);
    }

    private static string BuildChecklist(GameState state)
    {
        List<TutorialStepData> steps = TutorialService.GetTutorialSteps(state);
        StringBuilder builder = new StringBuilder();
        foreach (TutorialStepData step in steps)
        {
            if (step == null)
            {
                continue;
            }

            builder.Append(step.IsCompleted ? "[x] " : "[ ] ")
                .Append(step.Title)
                .Append(" - ")
                .Append(step.ActionLabel)
                .AppendLine();
        }

        return builder.ToString();
    }

    private static void SetText(Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }
}
