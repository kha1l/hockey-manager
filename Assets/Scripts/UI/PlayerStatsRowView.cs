using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsRowView : MonoBehaviour
{
    [SerializeField] private Text _text;

    public void Configure(Text text)
    {
        _text = text;
    }

    public void InitializeSkater(PlayerSeasonStatsData stats)
    {
        _text.text = stats.PlayerName
            + " | " + stats.Position
            + " | " + stats.GamesPlayed
            + " | " + stats.Goals
            + " | " + stats.Assists
            + " | " + stats.Points
            + " | ATOI " + IceTimeConfig.FormatSeconds(stats.AverageTimeOnIceSeconds)
            + " | PPP " + stats.PowerPlayPoints
            + " | PIM " + stats.PenaltyMinutes
            + " | " + stats.Shots
            + " | " + stats.PlusMinus;
    }

    public void InitializeGoalie(PlayerSeasonStatsData stats)
    {
        _text.text = stats.PlayerName
            + " | " + stats.GoalieGamesPlayed
            + " | " + stats.GoalieWins
            + " | " + stats.GoalieLosses
            + " | " + stats.GoalieOvertimeLosses
            + " | ATOI " + IceTimeConfig.FormatSeconds(stats.AverageTimeOnIceSeconds)
            + " | " + stats.Saves
            + " | " + stats.GoalsAgainst
            + " | " + stats.Shutouts;
    }

    public void InitializeMessage(string message)
    {
        _text.text = message;
    }
}
