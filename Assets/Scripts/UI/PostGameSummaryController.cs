using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class PostGameSummaryController : MonoBehaviour
{
    public Text TitleText;
    public Text ScoreText;
    public Text SummaryText;
    public Text StarsText;
    public Text EventsText;
    public Button DashboardButton;

    public void ShowSummary(PostGameSummaryData summary)
    {
        ApplyCompactSummaryUi();
        if (summary == null)
        {
            SetText(TitleText, "Итог матча");
            SetText(ScoreText, "Нет данных");
            return;
        }

        summary.EnsureCollections();
        SetText(TitleText, "Итог матча");
        SetText(ScoreText, CompactTeamName(summary.AwayTeamName)
            + " " + summary.AwayScore
            + " - " + summary.HomeScore
            + " " + CompactTeamName(summary.HomeTeamName));
        SetText(SummaryText, summary.Summary
            + "\nБроски: " + summary.AwayShots + " - " + summary.HomeShots
            + "\nБольшинство: " + summary.AwayPowerPlayGoals + "/" + summary.AwayPowerPlayOpportunities
            + " - " + summary.HomePowerPlayGoals + "/" + summary.HomePowerPlayOpportunities);
        SetText(StarsText, "Звёзды матча"
            + "\n1. " + summary.FirstStarPlayerName
            + "\n2. " + summary.SecondStarPlayerName
            + "\n3. " + summary.ThirdStarPlayerName);
        SetText(EventsText, BuildEvents(summary));
    }

    private void ApplyCompactSummaryUi()
    {
        ConfigureText(TitleText, new Vector2(0f, 675f), new Vector2(760f, 56f), 24, 34, TextAnchor.MiddleCenter);
        ConfigureText(ScoreText, new Vector2(0f, 612f), new Vector2(820f, 64f), 18, 28, TextAnchor.MiddleCenter);
        ConfigureText(SummaryText, new Vector2(0f, 470f), new Vector2(820f, 150f), 15, 19, TextAnchor.MiddleCenter);
        ConfigureText(StarsText, new Vector2(0f, 275f), new Vector2(820f, 135f), 15, 19, TextAnchor.MiddleCenter);
        ConfigureText(EventsText, new Vector2(0f, 45f), new Vector2(820f, 300f), 11, 14, TextAnchor.UpperLeft);

        Button dashboardButton = DashboardButton;
        if (dashboardButton == null)
        {
            Transform buttonTransform = transform.Find("DashboardButton");
            dashboardButton = buttonTransform == null ? null : buttonTransform.GetComponent<Button>();
        }

        if (dashboardButton != null)
        {
            DashboardButton = dashboardButton;
            RectTransform rect = dashboardButton.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = new Vector2(0f, -560f);
                rect.sizeDelta = new Vector2(360f, 60f);
            }

            Text buttonText = dashboardButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = "Вернуться в клуб";
                buttonText.resizeTextForBestFit = true;
                buttonText.resizeTextMinSize = 14;
                buttonText.resizeTextMaxSize = 22;
            }
        }
    }

    private static void ConfigureText(Text text, Vector2 anchoredPosition, Vector2 size, int minSize, int maxSize, TextAnchor alignment)
    {
        if (text == null)
        {
            return;
        }

        RectTransform rect = text.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }

        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = minSize;
        text.resizeTextMaxSize = maxSize;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
    }

    private static string BuildEvents(PostGameSummaryData summary)
    {
        StringBuilder builder = new StringBuilder();
        foreach (LiveMatchEventData matchEvent in summary.ScoringEvents)
        {
            if (matchEvent != null)
            {
                builder.AppendLine(matchEvent.PeriodLabel + " " + matchEvent.ClockLabel + "  " + matchEvent.Description);
            }
        }

        foreach (LiveMatchEventData matchEvent in summary.InjuryEvents)
        {
            if (matchEvent != null)
            {
                builder.AppendLine(matchEvent.Description);
            }
        }

        return builder.ToString();
    }

    private static string CompactTeamName(string teamName)
    {
        if (string.IsNullOrEmpty(teamName))
        {
            return "";
        }

        string compact = teamName.Replace("Saint Petersburg", "St. Petersburg");
        if (compact.Length <= 18)
        {
            return compact;
        }

        string[] parts = compact.Split(' ');
        if (parts.Length >= 2)
        {
            compact = parts[0] + " " + parts[parts.Length - 1];
        }

        return compact.Length <= 18 ? compact : compact.Substring(0, 18);
    }

    private static void SetText(Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }
}
