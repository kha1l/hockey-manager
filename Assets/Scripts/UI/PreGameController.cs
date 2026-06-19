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
    public Text HomeTeamInfoText;
    public Text AwayTeamInfoText;
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
        SetText(HomeTeamInfoText, setup.HomeTeamName + "\n" + setup.HomePreviewStatsText);
        SetText(AwayTeamInfoText, setup.AwayTeamName + "\n" + setup.AwayPreviewStatsText);
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
        AppendUserLineup(builder);
        builder.AppendLine();
        builder.AppendLine(setup.IsLineupValid ? "Состав готов" : setup.LineupValidationMessage);
        return builder.ToString();
    }

    private static void AppendUserLineup(StringBuilder builder)
    {
        TeamData team = GameSession.CurrentTeam;
        if (team == null)
        {
            return;
        }

        LineupService.EnsureLineup(team);
        if (team.Lineup == null)
        {
            return;
        }

        team.Lineup.EnsureCollections();
        builder.AppendLine("Звенья");
        foreach (ForwardLineData line in team.Lineup.ForwardLines)
        {
            if (line == null)
            {
                continue;
            }

            builder.AppendLine("F" + line.LineNumber + ": "
                + GetPlayerName(team, line.LeftWingPlayerId) + " - "
                + GetPlayerName(team, line.CenterPlayerId) + " - "
                + GetPlayerName(team, line.RightWingPlayerId));
        }

        builder.AppendLine("Пары защиты");
        foreach (DefensePairData pair in team.Lineup.DefensePairs)
        {
            if (pair == null)
            {
                continue;
            }

            builder.AppendLine("D" + pair.PairNumber + ": "
                + GetPlayerName(team, pair.LeftDefensePlayerId) + " - "
                + GetPlayerName(team, pair.RightDefensePlayerId));
        }
    }

    private static string GetPlayerName(TeamData team, string playerId)
    {
        if (team == null || string.IsNullOrEmpty(playerId))
        {
            return "-";
        }

        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            if (player != null && player.Id == playerId)
            {
                string number = player.JerseyNumber > 0 ? "#" + player.JerseyNumber + " " : "";
                return number + player.FirstName + " " + player.LastName;
            }
        }

        return "-";
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
