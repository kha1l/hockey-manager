using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class LiveMatchController : MonoBehaviour
{
    public Text ScoreboardText;
    public Text ClockText;
    public Text StatsText;
    public Text PowerPlayText;
    public Text EventFeedText;
    public Text TokenDetailsText;
    public Image TokenFullBodyImage;
    public RectTransform RinkArea;
    public LiveMatchPlayerTokenView TokenPrefab;
    public Button PauseButton;
    public Button Speed1Button;
    public Button Speed2Button;
    public Button Speed4Button;

    private float _tickAccumulator;

    public void ShowLiveMatch(LiveMatchStateData match)
    {
        _tickAccumulator = 0f;
        ApplyCompactMatchUi();
        Refresh(match);
    }

    public void TickUi(float deltaTime)
    {
        LiveMatchStateData match = GameSession.CurrentLiveMatch;
        if (match == null)
        {
            return;
        }

        _tickAccumulator += deltaTime;
        if (_tickAccumulator >= 1f)
        {
            _tickAccumulator = 0f;
            GameSession.TickLiveMatch(deltaTime);
        }

        Refresh(match);
    }

    public void TogglePause()
    {
        LiveMatchStateData match = GameSession.CurrentLiveMatch;
        if (match == null)
        {
            return;
        }

        GameSession.SetLiveMatchPaused(!match.IsPaused);
        Refresh(match);
    }

    public void SetSpeed1()
    {
        GameSession.SetLiveMatchSpeed(1);
    }

    public void SetSpeed2()
    {
        GameSession.SetLiveMatchSpeed(2);
    }

    public void SetSpeed4()
    {
        GameSession.SetLiveMatchSpeed(4);
    }

    public void SkipPeriod()
    {
        GameSession.SkipLiveMatchPeriod();
        Refresh(GameSession.CurrentLiveMatch);
    }

    public void SkipMatch()
    {
        GameSession.SkipLiveMatchToEnd();
        Refresh(GameSession.CurrentLiveMatch);
    }

    public void SetBalancedTactic()
    {
        SetUserTactic("Balanced");
    }

    public void SetOffensiveTactic()
    {
        SetUserTactic("Offensive");
    }

    public void SetDefensiveTactic()
    {
        SetUserTactic("Defensive");
    }

    public void SetAggressiveTactic()
    {
        SetUserTactic("Aggressive");
    }

    public void ChangeUserGoalie()
    {
        TeamData team = GameSession.CurrentTeam;
        if (team != null)
        {
            GameSession.ChangeLiveMatchGoalie(team.Id, out string message);
            Debug.Log(message);
        }
    }

    public void PullUserGoalie()
    {
        TeamData team = GameSession.CurrentTeam;
        if (team != null)
        {
            GameSession.PullLiveMatchGoalie(team.Id, out string message);
            Debug.Log(message);
        }
    }

    public void ReturnUserGoalie()
    {
        TeamData team = GameSession.CurrentTeam;
        if (team != null)
        {
            GameSession.ReturnLiveMatchGoalie(team.Id, out string message);
            Debug.Log(message);
        }
    }

    public void ShowTokenDetails(LiveMatchPlayerTokenData token)
    {
        if (token == null)
        {
            return;
        }

        if (TokenDetailsText != null)
        {
            TokenDetailsText.text = token.PlayerName
                + "\n#" + token.JerseyNumber + " " + token.Position
                + "\nCondition: " + token.Condition
                + "\nMorale: " + token.Morale;
        }

        if (TokenFullBodyImage != null)
        {
            Sprite sprite = string.IsNullOrEmpty(token.FullBodyResourcePath) ? null : TeamAssetService.LoadSprite(token.FullBodyResourcePath);
            TokenFullBodyImage.sprite = sprite;
            TokenFullBodyImage.enabled = sprite != null;
        }
    }

    private void Refresh(LiveMatchStateData match)
    {
        if (match == null)
        {
            return;
        }

        SetText(ScoreboardText, FormatScoreboard(match));
        SetText(ClockText, LiveMatchConfig.FormatPeriodLabel(match)
            + " | " + LiveMatchConfig.FormatClock(match.PeriodSecondsRemaining)
            + " | x" + match.SpeedMultiplier
            + (match.IsPaused ? " | Пауза" : ""));
        SetText(StatsText, "Броски: " + match.AwayStats.Shots + " - " + match.HomeStats.Shots
            + "\nШтраф: " + match.AwayStats.PenaltyMinutes + " - " + match.HomeStats.PenaltyMinutes);
        SetText(PowerPlayText, BuildPowerPlayText(match));
        SetText(EventFeedText, BuildEventFeed(match));
        RefreshTokens(match);
    }

    private void ApplyCompactMatchUi()
    {
        TintRink();
        ConfigureText(ScoreboardText, new Vector2(0f, 640f), new Vector2(820f, 58f), 18, 26, TextAnchor.MiddleCenter);
        ConfigureText(ClockText, new Vector2(0f, 594f), new Vector2(820f, 38f), 16, 21, TextAnchor.MiddleCenter);
        ConfigureText(StatsText, new Vector2(-220f, 530f), new Vector2(360f, 70f), 14, 17, TextAnchor.MiddleCenter);
        ConfigureText(PowerPlayText, new Vector2(220f, 530f), new Vector2(360f, 70f), 14, 17, TextAnchor.MiddleCenter);
        ConfigureText(EventFeedText, new Vector2(-220f, -210f), new Vector2(400f, 290f), 11, 13, TextAnchor.UpperLeft);
        if (EventFeedText != null)
        {
            EventFeedText.supportRichText = true;
        }
        ConfigureText(TokenDetailsText, new Vector2(245f, -170f), new Vector2(360f, 120f), 12, 14, TextAnchor.UpperLeft);
        SetRect(RinkArea, new Vector2(0f, 150f), new Vector2(820f, 520f));
        SetButtonVisible("Speed1Button", false);
        SetButtonVisible("LiveBalancedButton", false);
        SetButtonVisible("LiveOffensiveButton", false);
        SetButtonVisible("LiveDefensiveButton", false);
        SetButtonVisible("LiveAggressiveButton", false);
        SetButtonVisible("ChangeGoalieButton", false);
        SetButtonVisible("PullGoalieButton", false);
        SetButtonVisible("ReturnGoalieButton", false);
        SetButtonRect("PauseButton", true, new Vector2(-330f, -520f), new Vector2(160f, 48f));
        SetButtonRect("Speed2Button", true, new Vector2(-160f, -520f), new Vector2(130f, 48f));
        SetButtonRect("Speed4Button", true, new Vector2(-20f, -520f), new Vector2(130f, 48f));
        SetButtonRect("SkipPeriodButton", true, new Vector2(185f, -520f), new Vector2(260f, 48f));
        SetButtonRect("SkipMatchButton", true, new Vector2(-125f, -580f), new Vector2(260f, 48f));
        SetButtonRect("FinishLiveMatchButton", true, new Vector2(165f, -580f), new Vector2(220f, 48f));
        SetButtonVisible("ExitLiveMatchButton", false);
    }

    private static void ConfigureText(Text text, Vector2 anchoredPosition, Vector2 size, int minSize, int maxSize, TextAnchor alignment)
    {
        if (text == null)
        {
            return;
        }

        RectTransform rect = text.GetComponent<RectTransform>();
        SetRect(rect, anchoredPosition, size);
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = minSize;
        text.resizeTextMaxSize = maxSize;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
    }

    private static void SetRect(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private void TintRink()
    {
        if (RinkArea == null)
        {
            return;
        }

        Image image = RinkArea.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0.055f, 0.115f, 0.145f, 0.88f);
        }
    }

    private void SetButtonVisible(string objectName, bool isVisible)
    {
        Transform child = transform.Find(objectName);
        if (child != null)
        {
            child.gameObject.SetActive(isVisible);
        }
    }

    private void SetButtonRect(string objectName, bool isVisible, Vector2 anchoredPosition, Vector2 size)
    {
        Transform child = transform.Find(objectName);
        if (child == null)
        {
            return;
        }

        child.gameObject.SetActive(isVisible);
        RectTransform rect = child.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }
    }

    private void RefreshTokens(LiveMatchStateData match)
    {
        if (RinkArea == null || TokenPrefab == null)
        {
            return;
        }

        for (int i = RinkArea.childCount - 1; i >= 0; i--)
        {
            Destroy(RinkArea.GetChild(i).gameObject);
        }

        if (match.Tokens == null)
        {
            return;
        }

        foreach (LiveMatchPlayerTokenData token in match.Tokens)
        {
            LiveMatchPlayerTokenView view = Instantiate(TokenPrefab, RinkArea);
            RectTransform rect = view.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(token.NormalizedX, token.NormalizedY);
                rect.anchorMax = rect.anchorMin;
                rect.anchoredPosition = Vector2.zero;
            }

            view.gameObject.SetActive(true);
            view.Initialize(token, this);
        }
    }

    private static string BuildPowerPlayText(LiveMatchStateData match)
    {
        if (match.HomeStats.PowerPlaySecondsRemaining > 0)
        {
            return "";
        }

        if (match.AwayStats.PowerPlaySecondsRemaining > 0)
        {
            return "";
        }

        return "Равные составы";
    }

    private static string FormatScoreboard(LiveMatchStateData match)
    {
        if (match == null)
        {
            return "";
        }

        return CompactTeamName(match.AwayTeamName)
            + " " + match.AwayScore
            + " - " + match.HomeScore
            + " " + CompactTeamName(match.HomeTeamName);
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

    private static string BuildEventFeed(LiveMatchStateData match)
    {
        StringBuilder builder = new StringBuilder();
        if (match.Events == null)
        {
            return "";
        }

        int count = 0;
        foreach (LiveMatchEventData matchEvent in match.Events)
        {
            if (matchEvent == null)
            {
                continue;
            }

            if (matchEvent.EventType == "Goal")
            {
                builder.AppendLine("<color=#FFD84A>" + matchEvent.Description + "</color>");
            }
            else
            {
                builder.AppendLine(matchEvent.PeriodLabel + " " + matchEvent.ClockLabel + "  " + matchEvent.Description);
            }
            count++;
            if (count >= 10)
            {
                break;
            }
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

    private static void SetUserTactic(string tacticName)
    {
        TeamData team = GameSession.CurrentTeam;
        if (team != null)
        {
            GameSession.SetLiveMatchTactic(team.Id, tacticName, out string message);
            Debug.Log(message);
        }
    }
}
