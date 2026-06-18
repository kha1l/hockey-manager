using UnityEngine;
using UnityEngine.UI;

public class SeasonHistoryRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(SeasonHistoryData history)
    {
        if (_infoText == null)
        {
            return;
        }

        if (history == null)
        {
            _infoText.text = "История сезона недоступна";
            return;
        }

        string madePlayoffsText = history.UserTeamMadePlayoffs ? "да" : "нет";
        _infoText.text = FormatSeason(history.SeasonStartYear, history.SeasonEndYear)
            + " | Чемпион: " + SafeText(history.ChampionTeamName)
            + " | " + SafeText(history.UserTeamName)
            + " | Очки: " + history.UserTeamPoints
            + " | Место: " + history.UserTeamRank
            + " | Плей-офф: " + madePlayoffsText;
    }

    public void Initialize(LeagueSeasonHistoryData history)
    {
        if (_infoText == null)
        {
            return;
        }

        if (history == null)
        {
            _infoText.text = "История сезона недоступна";
            return;
        }

        _infoText.text = FormatSeason(history.SeasonStartYear, history.SeasonEndYear)
            + " | Champion: " + SafeText(history.ChampionTeamName)
            + " | Finalist: " + SafeText(history.FinalistTeamName)
            + " | Best RS: " + SafeText(history.BestRegularSeasonTeamName)
            + " (" + history.BestRegularSeasonPoints + " pts)"
            + " | Top scorer: " + SafeText(history.TopScorerPlayerName)
            + " " + history.TopScorerPoints + "P"
            + " | MVP: " + SafeText(history.MvpPlayerName)
            + " | User: " + SafeText(history.UserTeamResult);
    }

    private static string FormatSeason(int startYear, int endYear)
    {
        return startYear + "-" + (endYear % 100).ToString("D2");
    }

    private static string SafeText(string value)
    {
        return string.IsNullOrEmpty(value) ? "нет данных" : value;
    }
}
