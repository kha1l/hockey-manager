using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class PreGameController : MonoBehaviour
{
    public Text TitleText;
    public Text MatchupText;
    public Text DetailsText;
    public Text LineupText;
    public Text TacticsText;
    public Image HomeLogoImage;
    public Image AwayLogoImage;
    public Image HomeJerseyImage;
    public Image AwayJerseyImage;
    public Button StartButton;

    public void ShowPreGame(PreGameSetupData setup)
    {
        if (setup == null)
        {
            SetText(TitleText, "Матч не найден");
            return;
        }

        SetText(TitleText, setup.IsPlayoffGame ? "Матч плей-офф" : "Следующий матч");
        SetText(MatchupText, setup.Summary);
        SetText(DetailsText, setup.AvailabilityMessage);
        SetText(TacticsText, "Тактика: " + setup.CurrentTacticName);
        SetText(LineupText, BuildLineupText(setup));
        LoadImage(HomeLogoImage, setup.HomeLogoResourcePath);
        LoadImage(AwayLogoImage, setup.AwayLogoResourcePath);
        LoadImage(HomeJerseyImage, setup.HomeJerseyResourcePath);
        LoadImage(AwayJerseyImage, setup.AwayJerseyResourcePath);
        if (StartButton != null)
        {
            StartButton.interactable = setup.CanStartMatch;
        }
    }

    private static string BuildLineupText(PreGameSetupData setup)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Вратари");
        builder.AppendLine("Старт: " + setup.StartingGoalieName);
        builder.AppendLine("Запас: " + setup.BackupGoalieName);
        builder.AppendLine();
        builder.AppendLine(setup.IsLineupValid ? "Состав готов" : setup.LineupValidationMessage);
        return builder.ToString();
    }

    private static void SetText(Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }

    private static void LoadImage(Image image, string resourcePath)
    {
        if (image == null)
        {
            return;
        }

        Sprite sprite = string.IsNullOrEmpty(resourcePath) ? null : TeamAssetService.LoadSprite(resourcePath);
        image.sprite = sprite;
        image.enabled = sprite != null;
    }
}
