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
        InitializeSkater(stats, null);
    }

    public void InitializeSkater(PlayerSeasonStatsData stats, PlayerData player)
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
            + " | " + stats.PlusMinus
            + FormatSkaterCareer(player);
    }

    public void InitializeGoalie(PlayerSeasonStatsData stats)
    {
        InitializeGoalie(stats, null);
    }

    public void InitializeGoalie(PlayerSeasonStatsData stats, PlayerData player)
    {
        _text.text = stats.PlayerName
            + " | " + stats.GoalieGamesPlayed
            + " | " + stats.GoalieWins
            + " | " + stats.GoalieLosses
            + " | " + stats.GoalieOvertimeLosses
            + " | ATOI " + IceTimeConfig.FormatSeconds(stats.AverageTimeOnIceSeconds)
            + " | " + stats.Saves
            + " | " + stats.GoalsAgainst
            + " | " + stats.Shutouts
            + FormatGoalieCareer(player);
    }

    public void InitializeMessage(string message)
    {
        _text.text = message;
    }

    private static string FormatSkaterCareer(PlayerData player)
    {
        if (player == null || player.CareerGamesPlayed <= 0)
        {
            return "";
        }

        return " | CAR " + player.CareerGoals + "G "
            + player.CareerAssists + "A "
            + player.CareerPoints + "P"
            + " | AW " + player.CareerAwardsCount;
    }

    private static string FormatGoalieCareer(PlayerData player)
    {
        if (player == null || player.CareerGamesPlayed <= 0)
        {
            return "";
        }

        return " | CAR " + player.CareerWins + "W "
            + player.CareerShutouts + "SO"
            + " | AW " + player.CareerAwardsCount;
    }
}
