using System.Collections.Generic;
using UnityEngine;

public static class PlayerGameStatsGenerator
{
    public static List<PlayerGameStatData> Generate(MatchResultData result, TeamData homeTeam, TeamData awayTeam)
    {
        List<PlayerGameStatData> gameStats = new List<PlayerGameStatData>();

        if (result == null || homeTeam == null || awayTeam == null)
        {
            return gameStats;
        }

        EnsurePlayers(homeTeam);
        EnsurePlayers(awayTeam);
        PlayerFatigueService.EnsureFatigueForTeam(homeTeam);
        PlayerFatigueService.EnsureFatigueForTeam(awayTeam);
        InjuryService.EnsureInjuryFieldsForTeam(homeTeam);
        InjuryService.EnsureInjuryFieldsForTeam(awayTeam);
        PlayerRoleService.EnsureRolesForTeam(homeTeam);
        PlayerRoleService.EnsureRolesForTeam(awayTeam);
        IceTimeService.EnsureUsageForTeam(homeTeam);
        IceTimeService.EnsureUsageForTeam(awayTeam);
        ChemistryService.EnsureChemistryForTeam(homeTeam);
        ChemistryService.EnsureChemistryForTeam(awayTeam);

        GenerateTeamStats(result, homeTeam, true, gameStats);
        GenerateTeamStats(result, awayTeam, false, gameStats);

        foreach (PlayerGameStatData stat in gameStats)
        {
            stat.Points = stat.Goals + stat.Assists;
            stat.PowerPlayPoints = stat.PowerPlayGoals + stat.PowerPlayAssists;
            stat.ShortHandedPoints = stat.ShortHandedGoals + stat.ShortHandedAssists;
        }

        IncrementNhlGamesPlayed(homeTeam, gameStats);
        IncrementNhlGamesPlayed(awayTeam, gameStats);

        return gameStats;
    }

    private static void GenerateTeamStats(MatchResultData result, TeamData team, bool isHomeTeam, List<PlayerGameStatData> gameStats)
    {
        List<PlayerData> skaters = GetSkaters(team);
        PlayerData startingGoalie = GetStartingGoalie(team);
        int teamGoals = isHomeTeam ? result.HomeScore : result.AwayScore;
        int teamPowerPlayGoals = isHomeTeam ? result.HomePowerPlayGoals : result.AwayPowerPlayGoals;
        int teamPenaltyMinutes = isHomeTeam ? result.HomePenaltyMinutes : result.AwayPenaltyMinutes;
        int teamShots = isHomeTeam ? result.HomeShots : result.AwayShots;
        bool teamWon = result.WinnerTeamId == team.Id;
        int plusMinus = teamWon ? 1 : -1;

        List<PlayerGameStatData> skaterStats = new List<PlayerGameStatData>();
        foreach (PlayerData skater in skaters)
        {
            PlayerGameStatData stat = CreateBaseStat(skater, false);
            stat.PlusMinus = plusMinus;
            skaterStats.Add(stat);
            gameStats.Add(stat);
        }

        AssignGoalsAndAssists(team, teamGoals, teamPowerPlayGoals, skaters, skaterStats);
        AssignShots(team, teamShots, skaters, skaterStats);
        AssignPenaltyMinutes(team, teamPenaltyMinutes, skaters, skaterStats);

        if (startingGoalie != null)
        {
            PlayerGameStatData goalieStat = CreateBaseStat(startingGoalie, true);
            int shotsAgainst = isHomeTeam ? result.AwayShots : result.HomeShots;
            int goalsAgainst = isHomeTeam ? result.AwayScore : result.HomeScore;

            goalieStat.ShotsAgainst = shotsAgainst;
            goalieStat.GoalsAgainst = goalsAgainst;
            goalieStat.Saves = Mathf.Max(0, shotsAgainst - goalsAgainst);
            goalieStat.GoalieWin = teamWon;
            goalieStat.GoalieOvertimeLoss = !teamWon && result.IsOvertime;
            goalieStat.GoalieLoss = !teamWon && !result.IsOvertime;
            goalieStat.Shutout = goalsAgainst == 0;

            gameStats.Add(goalieStat);
        }
    }

    private static void AssignGoalsAndAssists(TeamData team, int goals, int powerPlayGoals, List<PlayerData> skaters, List<PlayerGameStatData> skaterStats)
    {
        for (int i = 0; i < goals; i++)
        {
            bool isPowerPlayGoal = i < powerPlayGoals;
            PlayerData scorer = isPowerPlayGoal
                ? PickWeightedPowerPlaySkater(team, skaters, null, "Goal")
                : PickWeightedSkater(team, skaters, null, "Goal");
            if (scorer == null)
            {
                return;
            }

            PlayerGameStatData scorerStat = FindStat(skaterStats, scorer.Id);
            if (scorerStat == null)
            {
                continue;
            }

            scorerStat.Goals++;
            if (isPowerPlayGoal)
            {
                scorerStat.PowerPlayGoals++;
            }

            int assistCount = Random.Range(0, 3);
            List<string> excludedPlayerIds = new List<string> { scorer.Id };

            for (int assistIndex = 0; assistIndex < assistCount; assistIndex++)
            {
                PlayerData assister = isPowerPlayGoal
                    ? PickWeightedPowerPlaySkater(team, skaters, excludedPlayerIds, "Assist")
                    : PickWeightedSkater(team, skaters, excludedPlayerIds, "Assist");
                if (assister == null)
                {
                    break;
                }

                PlayerGameStatData assisterStat = FindStat(skaterStats, assister.Id);
                if (assisterStat != null)
                {
                    assisterStat.Assists++;
                    if (isPowerPlayGoal)
                    {
                        assisterStat.PowerPlayAssists++;
                    }
                }

                excludedPlayerIds.Add(assister.Id);
            }
        }
    }

    private static void AssignPenaltyMinutes(TeamData team, int penaltyMinutes, List<PlayerData> skaters, List<PlayerGameStatData> skaterStats)
    {
        int penalties = Mathf.Max(0, penaltyMinutes / 2);
        for (int i = 0; i < penalties; i++)
        {
            PlayerData player = PickWeightedSkater(team, skaters, null, "Penalty");
            PlayerGameStatData stat = player == null ? null : FindStat(skaterStats, player.Id);
            if (stat != null)
            {
                stat.PenaltyMinutes += 2;
            }
        }
    }

    private static void AssignShots(TeamData team, int shots, List<PlayerData> skaters, List<PlayerGameStatData> skaterStats)
    {
        for (int i = 0; i < shots; i++)
        {
            PlayerData shooter = PickWeightedSkater(team, skaters, null, "Shot");
            if (shooter == null)
            {
                return;
            }

            PlayerGameStatData shooterStat = FindStat(skaterStats, shooter.Id);
            if (shooterStat != null)
            {
                shooterStat.Shots++;
            }
        }
    }

    private static PlayerGameStatData CreateBaseStat(PlayerData player, bool isGoalie)
    {
        return new PlayerGameStatData
        {
            PlayerId = player.Id,
            TeamId = player.TeamId,
            PlayerName = player.FirstName + " " + player.LastName,
            Position = player.Position,
            IsGoalie = isGoalie,
            TimeOnIceSeconds = GetStatTimeOnIce(player, isGoalie)
        };
    }

    private static List<PlayerData> GetSkaters(TeamData team)
    {
        LineupService.EnsureLineup(team);
        List<PlayerData> skaters = LineupService.GetActiveSkaters(team);
        skaters = FilterAvailablePlayers(skaters);
        if (skaters.Count > 0)
        {
            return skaters;
        }

        return GetFallbackSkaters(team);
    }

    private static PlayerData GetStartingGoalie(TeamData team)
    {
        LineupService.EnsureLineup(team);
        PlayerData goalie = LineupService.GetStartingGoalie(team);
        if (InjuryService.IsPlayerAvailable(goalie))
        {
            return goalie;
        }

        PlayerData backup = LineupService.GetBackupGoalie(team);
        if (InjuryService.IsPlayerAvailable(backup))
        {
            return backup;
        }

        List<PlayerData> goalies = new List<PlayerData>();
        foreach (PlayerData player in team.Players)
        {
            if (player != null
                && RosterStatusConfig.IsNhlRoster(player)
                && player.Position == "G"
                && InjuryService.IsPlayerAvailable(player))
            {
                goalies.Add(player);
            }
        }

        goalies.Sort((left, right) => right.Overall.CompareTo(left.Overall));
        return goalies.Count == 0 ? null : goalies[0];
    }

    private static PlayerData PickWeightedSkater(TeamData team, List<PlayerData> skaters, List<string> excludedPlayerIds, string context)
    {
        int totalWeight = 0;

        foreach (PlayerData skater in skaters)
        {
            if (IsExcluded(skater, excludedPlayerIds))
            {
                continue;
            }

            totalWeight += GetSkaterWeight(team, skater, context);
        }

        if (totalWeight <= 0)
        {
            return null;
        }

        int roll = Random.Range(0, totalWeight);
        foreach (PlayerData skater in skaters)
        {
            if (IsExcluded(skater, excludedPlayerIds))
            {
                continue;
            }

            roll -= GetSkaterWeight(team, skater, context);
            if (roll < 0)
            {
                return skater;
            }
        }

        return null;
    }

    private static PlayerData PickWeightedPowerPlaySkater(TeamData team, List<PlayerData> fallbackSkaters, List<string> excludedPlayerIds, string context)
    {
        List<PlayerData> powerPlayPlayers = SpecialTeamsService.GetPowerPlayPlayers(team);
        if (powerPlayPlayers.Count == 0)
        {
            return PickWeightedSkater(team, fallbackSkaters, excludedPlayerIds, context);
        }

        int totalWeight = 0;
        foreach (PlayerData player in powerPlayPlayers)
        {
            if (IsExcluded(player, excludedPlayerIds) || !InjuryService.IsPlayerAvailable(player))
            {
                continue;
            }

            totalWeight += GetPowerPlayWeight(team, player, context);
        }

        if (totalWeight <= 0)
        {
            return PickWeightedSkater(team, fallbackSkaters, excludedPlayerIds, context);
        }

        int roll = Random.Range(0, totalWeight);
        foreach (PlayerData player in powerPlayPlayers)
        {
            if (IsExcluded(player, excludedPlayerIds) || !InjuryService.IsPlayerAvailable(player))
            {
                continue;
            }

            roll -= GetPowerPlayWeight(team, player, context);
            if (roll < 0)
            {
                return player;
            }
        }

        return PickWeightedSkater(team, fallbackSkaters, excludedPlayerIds, context);
    }

    private static PlayerData PickRandomSkater(List<PlayerData> skaters)
    {
        if (skaters == null || skaters.Count == 0)
        {
            return null;
        }

        return skaters[Random.Range(0, skaters.Count)];
    }

    private static int GetSkaterWeight(TeamData team, PlayerData player, string context)
    {
        if (!RosterStatusConfig.IsNhlRoster(player))
        {
            return 0;
        }

        int positionBonus = player.Position == "D" ? 12 : 32;
        int usageWeight = team == null ? 0 : IceTimeService.GetUsageWeightForStats(team, player);
        if (usageWeight <= 0)
        {
            usageWeight = GetLineupWeightForPlayer(team, player);
        }

        if (usageWeight <= 0)
        {
            return 0;
        }

        float roleModifier = GetRoleModifier(player, context);
        float chemistryModifier = GetLineChemistryStatsModifier(team, player);
        int weight = Mathf.RoundToInt(usageWeight * (PlayerFatigueService.GetEffectiveOverall(player) + positionBonus) * roleModifier * chemistryModifier);
        return Mathf.Max(1, weight);
    }

    private static int GetPowerPlayWeight(TeamData team, PlayerData player, string context)
    {
        if (!RosterStatusConfig.IsNhlRoster(player) || !LineupService.IsPlayerActive(team, player.Id))
        {
            return 0;
        }

        int unitNumber = SpecialTeamsService.GetPowerPlayUnitNumberForPlayer(team, player.Id);
        int unitWeight = unitNumber == 1 ? 5 : 3;
        int positionBonus = player.Position == "D" ? 8 : 28;
        float roleModifier = GetRoleModifier(player, context);
        float chemistryModifier = GetPowerPlayChemistryStatsModifier(team, player);
        return Mathf.Max(1, Mathf.RoundToInt(unitWeight * (PlayerFatigueService.GetEffectiveOverall(player) + positionBonus) * roleModifier * chemistryModifier));
    }

    private static float GetLineChemistryStatsModifier(TeamData team, PlayerData player)
    {
        if (team == null || player == null)
        {
            return 1f;
        }

        if (team.Chemistry == null)
        {
            ChemistryService.EnsureChemistryForTeam(team);
        }

        TeamChemistryData chemistry = team.Chemistry;
        if (chemistry == null)
        {
            return 1f;
        }

        chemistry.EnsureCollections();
        foreach (LineChemistryData unit in chemistry.ForwardLines)
        {
            if (ContainsPlayer(unit, player.Id))
            {
                return GetChemistryStatsModifier(unit.ChemistryScore);
            }
        }

        foreach (LineChemistryData unit in chemistry.DefensePairs)
        {
            if (ContainsPlayer(unit, player.Id))
            {
                return GetChemistryStatsModifier(unit.ChemistryScore);
            }
        }

        return 1f;
    }

    private static float GetPowerPlayChemistryStatsModifier(TeamData team, PlayerData player)
    {
        if (team == null || player == null)
        {
            return 1f;
        }

        if (team.Chemistry == null)
        {
            ChemistryService.EnsureChemistryForTeam(team);
        }

        TeamChemistryData chemistry = team.Chemistry;
        if (chemistry == null)
        {
            return 1f;
        }

        chemistry.EnsureCollections();
        foreach (LineChemistryData unit in chemistry.PowerPlayUnits)
        {
            if (ContainsPlayer(unit, player.Id))
            {
                return GetChemistryStatsModifier(unit.ChemistryScore);
            }
        }

        return GetLineChemistryStatsModifier(team, player);
    }

    private static float GetChemistryStatsModifier(int score)
    {
        string label = ChemistryConfig.GetChemistryLabel(score);
        if (label == ChemistryConfig.LabelExcellent)
        {
            return 1.05f;
        }

        if (label == ChemistryConfig.LabelGood)
        {
            return 1.02f;
        }

        if (label == ChemistryConfig.LabelPoor)
        {
            return 0.97f;
        }

        return label == ChemistryConfig.LabelBad ? 0.94f : 1f;
    }

    private static bool ContainsPlayer(LineChemistryData unit, string playerId)
    {
        return unit != null
            && unit.PlayerIds != null
            && !string.IsNullOrEmpty(playerId)
            && unit.PlayerIds.Contains(playerId);
    }

    private static int GetStatTimeOnIce(PlayerData player, bool isGoalie)
    {
        if (player == null)
        {
            return 0;
        }

        int seconds = player.LastGameTimeOnIceSeconds > 0
            ? player.LastGameTimeOnIceSeconds
            : player.EstimatedTimeOnIceSeconds;

        if (isGoalie && seconds <= 0)
        {
            return IceTimeConfig.StartingGoalieSeconds;
        }

        return seconds;
    }

    private static float GetRoleModifier(PlayerData player, string context)
    {
        if (context == "Goal")
        {
            return PlayerRoleService.GetGoalScoringModifier(player);
        }

        if (context == "Assist")
        {
            return PlayerRoleService.GetAssistModifier(player);
        }

        if (context == "Shot")
        {
            return PlayerRoleService.GetShotModifier(player);
        }

        if (context == "Penalty")
        {
            return PlayerRoleService.GetPenaltyModifier(player);
        }

        return 1f;
    }

    private static int GetLineupWeightForPlayer(TeamData team, PlayerData player)
    {
        if (team == null || team.Lineup == null || player == null)
        {
            return 1;
        }

        team.Lineup.EnsureCollections();
        foreach (ForwardLineData line in team.Lineup.ForwardLines)
        {
            if (line == null)
            {
                continue;
            }

            if (line.LeftWingPlayerId == player.Id || line.CenterPlayerId == player.Id || line.RightWingPlayerId == player.Id)
            {
                if (line.LineNumber == 1)
                {
                    return 5;
                }

                if (line.LineNumber == 2)
                {
                    return 4;
                }

                if (line.LineNumber == 3)
                {
                    return 3;
                }

                return 2;
            }
        }

        foreach (DefensePairData pair in team.Lineup.DefensePairs)
        {
            if (pair == null)
            {
                continue;
            }

            if (pair.LeftDefensePlayerId == player.Id || pair.RightDefensePlayerId == player.Id)
            {
                if (pair.PairNumber == 1)
                {
                    return 4;
                }

                if (pair.PairNumber == 2)
                {
                    return 3;
                }

                return 2;
            }
        }

        return 1;
    }

    private static List<PlayerData> GetFallbackSkaters(TeamData team)
    {
        List<PlayerData> skaters = new List<PlayerData>();
        if (team == null || team.Players == null)
        {
            return skaters;
        }

        foreach (PlayerData player in team.Players)
        {
            if (player != null
                && RosterStatusConfig.IsNhlRoster(player)
                && player.Position != "G"
                && InjuryService.IsPlayerAvailable(player))
            {
                skaters.Add(player);
            }
        }

        skaters.Sort((left, right) => right.Overall.CompareTo(left.Overall));
        if (skaters.Count > LineupConfig.ActiveSkaterCount)
        {
            skaters.RemoveRange(LineupConfig.ActiveSkaterCount, skaters.Count - LineupConfig.ActiveSkaterCount);
        }

        return skaters;
    }

    private static bool IsExcluded(PlayerData player, List<string> excludedPlayerIds)
    {
        if (player == null || !RosterStatusConfig.IsNhlRoster(player) || !InjuryService.IsPlayerAvailable(player))
        {
            return true;
        }

        if (excludedPlayerIds == null)
        {
            return false;
        }

        return excludedPlayerIds.Contains(player.Id);
    }

    private static PlayerGameStatData FindStat(List<PlayerGameStatData> stats, string playerId)
    {
        foreach (PlayerGameStatData stat in stats)
        {
            if (stat.PlayerId == playerId)
            {
                return stat;
            }
        }

        return null;
    }

    private static void EnsurePlayers(TeamData team)
    {
        team.EnsurePlayers();

        if (team.Players.Count == 0)
        {
            team.Players = PlayerSeedData.CreatePlayersForTeam(team.Id);
        }

        PlayerFatigueService.EnsureFatigueForTeam(team);
        InjuryService.EnsureInjuryFieldsForTeam(team);
        TeamRosterService.EnsureRosterStatusesForTeam(team);
    }

    private static List<PlayerData> FilterAvailablePlayers(List<PlayerData> players)
    {
        List<PlayerData> availablePlayers = new List<PlayerData>();
        if (players == null)
        {
            return availablePlayers;
        }

        foreach (PlayerData player in players)
        {
            if (RosterStatusConfig.IsNhlRoster(player) && InjuryService.IsPlayerAvailable(player))
            {
                availablePlayers.Add(player);
            }
        }

        return availablePlayers;
    }

    private static void IncrementNhlGamesPlayed(TeamData team, List<PlayerGameStatData> gameStats)
    {
        if (team == null || gameStats == null)
        {
            return;
        }

        HashSet<string> countedPlayerIds = new HashSet<string>();
        foreach (PlayerGameStatData stat in gameStats)
        {
            if (stat == null || stat.TeamId != team.Id || string.IsNullOrEmpty(stat.PlayerId) || countedPlayerIds.Contains(stat.PlayerId))
            {
                continue;
            }

            PlayerData player = FindPlayer(team, stat.PlayerId);
            if (RosterStatusConfig.IsNhlRoster(player))
            {
                player.NHLGamesThisSeason++;
                countedPlayerIds.Add(stat.PlayerId);
            }
        }
    }

    private static PlayerData FindPlayer(TeamData team, string playerId)
    {
        if (team == null || string.IsNullOrEmpty(playerId))
        {
            return null;
        }

        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            if (player != null && player.Id == playerId)
            {
                return player;
            }
        }

        return null;
    }
}
