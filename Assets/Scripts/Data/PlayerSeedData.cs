using System.Collections.Generic;

public static class PlayerSeedData
{
    public static List<PlayerData> CreatePlayersForTeam(string teamId)
    {
        List<PlayerData> players = new List<PlayerData>();

        AddPlayers(players, teamId, "C", 4);
        AddPlayers(players, teamId, "LW", 4);
        AddPlayers(players, teamId, "RW", 4);
        AddPlayers(players, teamId, "D", 8);
        AddPlayers(players, teamId, "G", 3);

        return players;
    }

    private static void AddPlayers(List<PlayerData> players, string teamId, string position, int count)
    {
        for (int i = 0; i < count; i++)
        {
            int playerNumber = players.Count + 1;
            int overall = 60 + (playerNumber * 3 % 31);
            int potential = overall + (playerNumber * 2 % (96 - overall));

            players.Add(new PlayerData
            {
                Id = teamId + "-player-" + playerNumber.ToString("00"),
                FirstName = "Player",
                LastName = playerNumber.ToString(),
                TeamId = teamId,
                Position = position,
                Age = 19 + ((playerNumber - 1) % 18),
                Overall = overall,
                Potential = potential,
                Condition = FatigueConfig.DefaultCondition,
                Fatigue = FatigueConfig.DefaultFatigue,
                ConsecutiveGamesPlayed = 0,
                GamesRested = 0,
                IsResting = false,
                LastGameFatigueChange = 0,
                LastGameConditionChange = 0,
                IsInjured = false,
                InjuryType = "",
                InjurySeverity = "",
                InjuryDaysRemaining = 0,
                CanPlayThroughInjury = false,
                InjuredAtUtc = "",
                ExpectedReturnDate = "",
                TotalInjuries = 0
            });
        }
    }
}
