using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PostGameSummaryController : MonoBehaviour
{
    public Text TitleText;
    public Text ScoreText;
    public Text SummaryText;
    public Text StarsText;
    public Text EventsText;
    public Image HomeLogoImage;
    public Image AwayLogoImage;
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
        SetText(ScoreText, CompactTeamName(summary.HomeTeamName)
            + " " + summary.HomeScore
            + " - " + summary.AwayScore
            + " " + CompactTeamName(summary.AwayTeamName));
        SetText(SummaryText, summary.Summary
            + "\nБроски: " + summary.HomeShots + " - " + summary.AwayShots
            + "\nPP: " + FormatSpecialTeams(summary.HomePowerPlayGoals, summary.HomePowerPlayOpportunities)
            + " - " + FormatSpecialTeams(summary.AwayPowerPlayGoals, summary.AwayPowerPlayOpportunities)
            + "\nPK: " + FormatPenaltyKill(summary.AwayPowerPlayGoals, summary.AwayPowerPlayOpportunities)
            + " - " + FormatPenaltyKill(summary.HomePowerPlayGoals, summary.HomePowerPlayOpportunities));
        SetText(StarsText, "Звёзды матча"
            + "\n1. " + summary.FirstStarPlayerName
            + "\n2. " + summary.SecondStarPlayerName
            + "\n3. " + summary.ThirdStarPlayerName);
        SetText(EventsText, BuildEvents(summary));
        LoadLogo(HomeLogoImage, summary.HomeTeamId);
        LoadLogo(AwayLogoImage, summary.AwayTeamId);
    }

    private void ApplyCompactSummaryUi()
    {
        ConfigureText(TitleText, new Vector2(0f, 675f), new Vector2(760f, 56f), 24, 34, TextAnchor.MiddleCenter);
        ConfigureText(ScoreText, new Vector2(0f, 612f), new Vector2(820f, 64f), 18, 28, TextAnchor.MiddleCenter);
        ConfigureText(SummaryText, new Vector2(0f, 470f), new Vector2(820f, 150f), 15, 19, TextAnchor.MiddleCenter);
        ConfigureText(StarsText, new Vector2(0f, 275f), new Vector2(820f, 135f), 15, 19, TextAnchor.MiddleCenter);
        ConfigureText(EventsText, new Vector2(0f, 45f), new Vector2(820f, 300f), 11, 14, TextAnchor.UpperLeft);
        HomeLogoImage = EnsureLogoImage(HomeLogoImage, "HomeLogo", new Vector2(-360f, 600f));
        AwayLogoImage = EnsureLogoImage(AwayLogoImage, "AwayLogo", new Vector2(360f, 600f));
        if (EventsText != null)
        {
            EventsText.supportRichText = true;
        }

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

    private Image EnsureLogoImage(Image image, string objectName, Vector2 anchoredPosition)
    {
        if (image == null)
        {
            Transform existing = transform.Find(objectName);
            image = existing == null ? null : existing.GetComponent<Image>();
        }

        if (image == null)
        {
            GameObject logoObject = new GameObject(objectName);
            logoObject.transform.SetParent(transform, false);
            image = logoObject.AddComponent<Image>();
            image.preserveAspect = true;
        }

        RectTransform rect = image.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(92f, 92f);
        }

        return image;
    }

    private static string BuildEvents(PostGameSummaryData summary)
    {
        StringBuilder builder = new StringBuilder();
        List<LiveMatchEventData> events = new List<LiveMatchEventData>();
        AddEvents(events, summary.ScoringEvents);
        AddEvents(events, summary.PenaltyEvents);
        AddEvents(events, summary.InjuryEvents);
        events.Sort(CompareEventsAscending);
        foreach (LiveMatchEventData matchEvent in events)
        {
            builder.AppendLine(FormatEventLine(matchEvent));
        }

        return builder.ToString();
    }

    private static void AddEvents(List<LiveMatchEventData> target, List<LiveMatchEventData> source)
    {
        if (target == null || source == null)
        {
            return;
        }

        foreach (LiveMatchEventData matchEvent in source)
        {
            if (matchEvent != null)
            {
                target.Add(matchEvent);
            }
        }
    }

    private static int CompareEventsAscending(LiveMatchEventData left, LiveMatchEventData right)
    {
        int timeComparison = left.GameSecondsElapsed.CompareTo(right.GameSecondsElapsed);
        if (timeComparison != 0)
        {
            return timeComparison;
        }

        return right.Importance.CompareTo(left.Importance);
    }

    private static string FormatEventLine(LiveMatchEventData matchEvent)
    {
        string line = matchEvent.EventType == "Goal" || matchEvent.EventType == "ShootoutGoal"
            ? matchEvent.Description
            : matchEvent.PeriodLabel + " " + matchEvent.ClockLabel + "  " + matchEvent.Description;
        return matchEvent.EventType == "Goal"
            ? "<color=#FFD84A>" + line + "</color>"
            : line;
    }

    private static string FormatSpecialTeams(int goals, int opportunities)
    {
        return goals + "/" + opportunities + " " + FormatPercent(opportunities == 0 ? 0f : (float)goals / opportunities);
    }

    private static string FormatPenaltyKill(int powerPlayGoalsAgainst, int opportunitiesAgainst)
    {
        float value = opportunitiesAgainst == 0 ? 1f : 1f - (float)powerPlayGoalsAgainst / opportunitiesAgainst;
        return FormatPercent(value);
    }

    private static string FormatPercent(float value)
    {
        return Mathf.RoundToInt(Mathf.Clamp01(value) * 100f) + "%";
    }

    private static void LoadLogo(Image image, string teamId)
    {
        if (image == null)
        {
            return;
        }

        TeamData team = FindTeam(teamId);
        string path = team == null || team.Identity == null ? "" : team.Identity.LogoResourcePath;
        Sprite sprite = string.IsNullOrEmpty(path) ? null : TeamAssetService.LoadSprite(path);
        image.sprite = sprite;
        image.enabled = sprite != null;
    }

    private static TeamData FindTeam(string teamId)
    {
        if (GameSession.CurrentState == null || GameSession.CurrentState.Teams == null || string.IsNullOrEmpty(teamId))
        {
            return null;
        }

        foreach (TeamData team in GameSession.CurrentState.Teams)
        {
            if (team != null && team.Id == teamId)
            {
                return team;
            }
        }

        return null;
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
