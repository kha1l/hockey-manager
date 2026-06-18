using UnityEngine;
using UnityEngine.UI;

public class PlayoffSeriesRowView : MonoBehaviour
{
    [SerializeField] private Text _text;

    public void Configure(Text text)
    {
        _text = text;
    }

    public void Initialize(PlayoffSeriesData series)
    {
        if (_text == null || series == null)
        {
            return;
        }

        EnsureRowLayout();
        series.EnsureGames();
        string status = series.IsCompleted
            ? "Победитель: " + SafeText(series.WinnerTeamName)
            : "Идёт";
        string latestGames = BuildLatestGamesText(series);
        _text.text = series.RoundName
            + " | " + series.TeamAName + " " + series.TeamAWins
            + " - " + series.TeamBWins + " " + series.TeamBName
            + "\n" + status
            + latestGames;
    }

    private void EnsureRowLayout()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null && rectTransform.sizeDelta.y < 104f)
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 104f);
        }

        LayoutElement layoutElement = GetComponent<LayoutElement>();
        if (layoutElement != null)
        {
            layoutElement.minHeight = Mathf.Max(layoutElement.minHeight, 104f);
            layoutElement.preferredHeight = Mathf.Max(layoutElement.preferredHeight, 104f);
        }

        _text.fontSize = Mathf.Min(_text.fontSize, 14);
        _text.alignment = TextAnchor.MiddleLeft;

        RectTransform textRectTransform = _text.GetComponent<RectTransform>();
        if (textRectTransform != null)
        {
            textRectTransform.sizeDelta = new Vector2(textRectTransform.sizeDelta.x, Mathf.Max(textRectTransform.sizeDelta.y, 92f));
        }
    }

    private static string BuildLatestGamesText(PlayoffSeriesData series)
    {
        if (series == null || series.Games == null || series.Games.Count == 0)
        {
            return "\nМатчей пока нет";
        }

        int start = Mathf.Max(0, series.Games.Count - 3);
        string text = "";
        for (int i = start; i < series.Games.Count; i++)
        {
            MatchResultData game = series.Games[i];
            if (game == null)
            {
                continue;
            }

            text += "\nG" + (i + 1) + ": "
                + SafeText(game.HomeTeamName) + " " + game.HomeScore
                + " - " + game.AwayScore + " " + SafeText(game.AwayTeamName);
        }

        return text;
    }

    private static string SafeText(string value)
    {
        return string.IsNullOrEmpty(value) ? "Team" : value;
    }
}
