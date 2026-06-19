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
        _text.text = FormatPlayerName(stats, player)
            + " | " + stats.Position
            + FormatAge(player)
            + " | " + stats.GamesPlayed
            + " | " + stats.Goals
            + " | " + stats.Assists
            + " | " + stats.Points
            + " | ATOI " + IceTimeConfig.FormatSeconds(stats.AverageTimeOnIceSeconds)
            + " | PPP " + stats.PowerPlayPoints
            + " | PIM " + stats.PenaltyMinutes
            + " | " + stats.PlusMinus
            + FormatSkaterCareer(player);
    }

    public void InitializeGoalie(PlayerSeasonStatsData stats)
    {
        InitializeGoalie(stats, null);
    }

    public void InitializeGoalie(PlayerSeasonStatsData stats, PlayerData player)
    {
        _text.text = FormatPlayerName(stats, player)
            + FormatAge(player)
            + " | " + stats.GoalieGamesPlayed
            + " | " + stats.GoalieWins
            + " | " + stats.GoalieLosses
            + " | " + stats.GoalieOvertimeLosses
            + " | ATOI " + IceTimeConfig.FormatSeconds(stats.AverageTimeOnIceSeconds)
            + " | SV% " + FormatSavePercentage(stats)
            + " | GAA " + FormatGoalsAgainstAverage(stats)
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

    private static string FormatAge(PlayerData player)
    {
        return player == null || player.Age <= 0 ? "" : " | Age " + player.Age;
    }

    private static string FormatPlayerName(PlayerSeasonStatsData stats, PlayerData player)
    {
        if (player == null)
        {
            return stats == null ? "" : stats.PlayerName + FormatTeam(stats.TeamId);
        }

        string number = player.JerseyNumber > 0 ? "#" + player.JerseyNumber + " " : "";
        return number + player.FirstName + " " + player.LastName + FormatTeam(player);
    }

    private static string FormatTeam(PlayerData player)
    {
        return player == null ? "" : FormatTeam(player.TeamId);
    }

    private static string FormatTeam(string teamId)
    {
        if (string.IsNullOrEmpty(teamId) || GameSession.CurrentState == null || GameSession.CurrentState.Teams == null)
        {
            return "";
        }

        foreach (TeamData team in GameSession.CurrentState.Teams)
        {
            if (team != null && team.Id == teamId)
            {
                string abbreviation = TeamIdentityService.GetAbbreviation(team);
                return string.IsNullOrEmpty(abbreviation) ? "" : " [" + abbreviation + "]";
            }
        }

        return "";
    }

    private static string FormatSavePercentage(PlayerSeasonStatsData stats)
    {
        if (stats == null || stats.ShotsAgainst <= 0)
        {
            return ".000";
        }

        float savePercentage = Mathf.Clamp01(stats.Saves / (float)stats.ShotsAgainst);
        return savePercentage.ToString(".000");
    }

    private static string FormatGoalsAgainstAverage(PlayerSeasonStatsData stats)
    {
        if (stats == null || stats.GoalieGamesPlayed <= 0)
        {
            return "0.00";
        }

        float average = stats.GoalsAgainst / (float)stats.GoalieGamesPlayed;
        return average.ToString("0.00");
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
