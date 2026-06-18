using UnityEngine;
using UnityEngine.UI;

public class ScheduleGameRowView : MonoBehaviour
{
    [SerializeField] private Text _descriptionText;

    public void Configure(Text descriptionText)
    {
        _descriptionText = descriptionText;
    }

    public void Initialize(ScheduleGameData game)
    {
        _descriptionText.text = GetPrefix(game) + "День " + game.DayNumber + ". " + GetMatchText(game);
    }

    public void InitializeMessage(string message)
    {
        if (_descriptionText != null)
        {
            _descriptionText.text = message;
        }
    }

    private static string GetMatchText(ScheduleGameData game)
    {
        if (!game.IsPlayed || game.Result == null)
        {
            return game.HomeTeamName + " vs " + game.AwayTeamName + " — Не сыгран";
        }

        string status = game.HomeTeamName + " " + game.Result.HomeScore + " - " + game.Result.AwayScore + " " + game.AwayTeamName;
        if (game.Result.IsOvertime)
        {
            status += " OT";
        }

        return status;
    }

    private static string GetPrefix(ScheduleGameData game)
    {
        TeamData currentTeam = GameSession.CurrentTeam;
        if (currentTeam == null)
        {
            return "";
        }

        return game.HomeTeamId == currentTeam.Id || game.AwayTeamId == currentTeam.Id ? "* " : "";
    }
}
