using System;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerDevelopmentService
{
    private static HashSet<string> _processedDevelopmentKeys;

    public static void EnsureDevelopmentHistory(GameState state)
    {
        if (state == null)
        {
            return;
        }

        if (state.PlayerDevelopmentHistory == null)
        {
            state.PlayerDevelopmentHistory = new PlayerDevelopmentHistoryData();
        }

        state.PlayerDevelopmentHistory.EnsureChanges();
    }

    public static void ApplyYearlyDevelopment(GameState state)
    {
        EnsureDevelopmentHistory(state);
        if (state == null || state.CurrentSeasonStartYear <= 0)
        {
            return;
        }

        if (state.PlayerDevelopmentHistory.LastProcessedSeasonStartYear == state.CurrentSeasonStartYear)
        {
            Debug.Log("Development already processed for this season");
            return;
        }

        int changesBefore = state.PlayerDevelopmentHistory.Changes.Count;
        _processedDevelopmentKeys = new HashSet<string>();

        ApplyDevelopmentToRosterPlayers(state);
        ApplyDevelopmentToFreeAgents(state);
        ApplyDevelopmentToDraftRights(state);

        _processedDevelopmentKeys = null;
        state.PlayerDevelopmentHistory.LastProcessedSeasonStartYear = state.CurrentSeasonStartYear;

        int createdChanges = state.PlayerDevelopmentHistory.Changes.Count - changesBefore;
        Debug.Log("Player development processed: " + createdChanges + " changes");
    }

    public static void ApplyDevelopmentToRosterPlayers(GameState state)
    {
        EnsureDevelopmentHistory(state);
        if (state == null || state.Teams == null)
        {
            return;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team == null)
            {
                continue;
            }

            team.EnsurePlayers();
            string teamName = GetTeamName(team);
            foreach (PlayerData player in team.Players)
            {
                PlayerDevelopmentChangeData change = ApplyDevelopmentToPlayer(
                    state,
                    player,
                    "RosterPlayer",
                    team.Id,
                    teamName);

                AddChange(state, change);
            }
        }
    }

    public static void ApplyDevelopmentToFreeAgents(GameState state)
    {
        EnsureDevelopmentHistory(state);
        if (state == null || state.FreeAgentPool == null || state.FreeAgentPool.FreeAgents == null)
        {
            return;
        }

        foreach (PlayerData player in state.FreeAgentPool.FreeAgents)
        {
            if (player != null)
            {
                player.Age++;
            }

            PlayerDevelopmentChangeData change = ApplyDevelopmentToPlayer(
                state,
                player,
                "FreeAgent",
                "free-agents",
                "Free Agents");

            AddChange(state, change);
        }
    }

    public static void ApplyDevelopmentToDraftRights(GameState state)
    {
        EnsureDevelopmentHistory(state);
        if (state == null || state.Teams == null)
        {
            return;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team == null)
            {
                continue;
            }

            team.EnsureDraftRights();
            string teamName = GetTeamName(team);
            foreach (ProspectData prospect in team.DraftRights)
            {
                if (prospect != null)
                {
                    prospect.Age++;
                }

                PlayerDevelopmentChangeData change = ApplyDevelopmentToProspect(
                    state,
                    prospect,
                    team.Id,
                    teamName);

                AddChange(state, change);
            }
        }
    }

    private static PlayerDevelopmentChangeData ApplyDevelopmentToPlayer(
        GameState state,
        PlayerData player,
        string entityType,
        string teamId,
        string teamName)
    {
        if (state == null || player == null || IsAlreadyProcessed(entityType, player.Id))
        {
            return null;
        }

        MarkProcessed(entityType, player.Id);

        int oldOverall = player.Overall;
        int oldPotential = player.Potential;
        int delta = CalculatePlayerDelta(player, state.CurrentSeasonStartYear);
        int potentialDelta = CalculatePotentialDelta(
            player.Age,
            oldOverall,
            oldPotential,
            state.CurrentSeasonStartYear,
            player.Id);
        int newOverall = PlayerDevelopmentConfig.ClampOverall(oldOverall + delta);
        int newPotential = PlayerDevelopmentConfig.ClampPotential(oldPotential + potentialDelta);

        if (newOverall > newPotential)
        {
            newPotential = newOverall;
        }

        string developmentType = GetDevelopmentType(newOverall - oldOverall);

        player.LastSeasonOverall = oldOverall;
        player.LastSeasonPotential = oldPotential;
        player.Overall = newOverall;
        player.Potential = newPotential;
        player.LastDevelopmentDelta = newOverall - oldOverall;
        player.LastDevelopmentType = developmentType;

        if (player.LastDevelopmentDelta == 0 && newPotential - oldPotential == 0)
        {
            return null;
        }

        return CreateChange(
            state,
            entityType,
            player.Id,
            player.FirstName + " " + player.LastName,
            teamId,
            teamName,
            player.Position,
            player.Age,
            oldOverall,
            newOverall,
            oldPotential,
            newPotential,
            developmentType,
            GetReason(entityType, player.Age, player.LastDevelopmentDelta));
    }

    private static PlayerDevelopmentChangeData ApplyDevelopmentToProspect(
        GameState state,
        ProspectData prospect,
        string teamId,
        string teamName)
    {
        if (state == null || prospect == null || IsAlreadyProcessed("DraftRightsProspect", prospect.Id))
        {
            return null;
        }

        MarkProcessed("DraftRightsProspect", prospect.Id);

        int oldOverall = prospect.Overall;
        int oldPotential = prospect.Potential;
        int delta = CalculateProspectDelta(prospect, state.CurrentSeasonStartYear);
        int potentialDelta = CalculatePotentialDelta(
            prospect.Age,
            oldOverall,
            oldPotential,
            state.CurrentSeasonStartYear,
            prospect.Id);
        int newOverall = PlayerDevelopmentConfig.ClampOverall(oldOverall + delta);
        int newPotential = PlayerDevelopmentConfig.ClampPotential(oldPotential + potentialDelta);

        if (newOverall > newPotential)
        {
            newPotential = newOverall;
        }

        string developmentType = GetDevelopmentType(newOverall - oldOverall);

        prospect.LastSeasonOverall = oldOverall;
        prospect.LastSeasonPotential = oldPotential;
        prospect.Overall = newOverall;
        prospect.Potential = newPotential;
        prospect.LastDevelopmentDelta = newOverall - oldOverall;
        prospect.LastDevelopmentType = developmentType;

        if (prospect.LastDevelopmentDelta == 0 && newPotential - oldPotential == 0)
        {
            return null;
        }

        return CreateChange(
            state,
            "DraftRightsProspect",
            prospect.Id,
            prospect.FirstName + " " + prospect.LastName,
            teamId,
            teamName,
            prospect.Position,
            prospect.Age,
            oldOverall,
            newOverall,
            oldPotential,
            newPotential,
            developmentType,
            GetReason("DraftRightsProspect", prospect.Age, prospect.LastDevelopmentDelta));
    }

    private static int CalculatePlayerDelta(PlayerData player, int seasonSeed)
    {
        if (player == null)
        {
            return 0;
        }

        int roll = StableRange(player.Id + ":" + seasonSeed + ":" + player.Age + ":player-delta", 0, 99);
        int growthRoom = player.Potential - player.Overall;
        int delta;

        if (player.Age <= PlayerDevelopmentConfig.ProspectFastGrowthMaxAge)
        {
            delta = WeightedDelta(roll, 0, 1, 2, 3, 4, PlayerDevelopmentConfig.MaxYearlyGrowth);
        }
        else if (player.Age <= PlayerDevelopmentConfig.YoungGrowthMaxAge)
        {
            delta = WeightedDelta(roll, -1, 0, 1, 2, 3, 4);
        }
        else if (player.Age <= PlayerDevelopmentConfig.SlowGrowthMaxAge)
        {
            delta = WeightedDelta(roll, -1, 0, 0, 1, 1, 2);
        }
        else if (player.Age <= PlayerDevelopmentConfig.PrimeAgeMax)
        {
            delta = WeightedDelta(roll, -1, -1, 0, 0, 1, 1);
        }
        else if (player.Age < PlayerDevelopmentConfig.HeavyRegressionStartAge)
        {
            delta = WeightedDelta(roll, -3, -2, -1, -1, 0, 1);
        }
        else
        {
            delta = WeightedDelta(roll, PlayerDevelopmentConfig.MaxYearlyRegression, -4, -3, -2, -1, 0);
        }

        if (growthRoom >= 10 && player.Age <= PlayerDevelopmentConfig.SlowGrowthMaxAge)
        {
            delta += 1;
        }

        if (growthRoom <= 2 && delta > 1)
        {
            delta = 1;
        }

        if (player.Overall >= player.Potential && delta > 1)
        {
            delta = 1;
        }

        if (player.Age >= PlayerDevelopmentConfig.VeteranRegressionStartAge && delta > 1)
        {
            delta = 1;
        }

        if (player.Age >= PlayerDevelopmentConfig.HeavyRegressionStartAge && delta > 0)
        {
            delta = 0;
        }

        return ClampDelta(delta, PlayerDevelopmentConfig.MaxYearlyRegression, PlayerDevelopmentConfig.MaxYearlyGrowth);
    }

    private static int CalculateProspectDelta(ProspectData prospect, int seasonSeed)
    {
        if (prospect == null)
        {
            return 0;
        }

        int roll = StableRange(prospect.Id + ":" + seasonSeed + ":" + prospect.Age + ":prospect-delta", 0, 99);
        int growthRoom = prospect.Potential - prospect.Overall;
        int delta;

        if (prospect.Age <= PlayerDevelopmentConfig.ProspectFastGrowthMaxAge)
        {
            delta = WeightedDelta(roll, 0, 1, 2, 3, 4, PlayerDevelopmentConfig.MaxProspectYearlyGrowth);
        }
        else if (prospect.Age <= PlayerDevelopmentConfig.YoungGrowthMaxAge)
        {
            delta = WeightedDelta(roll, -1, 0, 1, 2, 3, 4);
        }
        else if (prospect.Age == 24)
        {
            delta = WeightedDelta(roll, -1, 0, 0, 1, 1, 2);
        }
        else
        {
            delta = WeightedDelta(roll, -3, -2, -1, 0, 0, 1);
        }

        if (growthRoom >= 15)
        {
            delta += 1;
        }

        if (prospect.Potential >= 90 && prospect.Age <= PlayerDevelopmentConfig.YoungGrowthMaxAge)
        {
            delta += 1;
        }

        if (growthRoom <= 2 && delta > 1)
        {
            delta = 1;
        }

        if (prospect.Overall >= prospect.Potential && delta > 1)
        {
            delta = 1;
        }

        return ClampDelta(delta, PlayerDevelopmentConfig.MaxYearlyRegression, PlayerDevelopmentConfig.MaxProspectYearlyGrowth);
    }

    private static int CalculatePotentialDelta(int age, int overall, int potential, int seasonSeed, string id)
    {
        int roll = StableRange(id + ":" + seasonSeed + ":" + age + ":potential-delta", 0, 99);
        int growthRoom = potential - overall;

        if (age >= PlayerDevelopmentConfig.HeavyRegressionStartAge && roll < 25)
        {
            return -1;
        }

        if (age >= PlayerDevelopmentConfig.VeteranRegressionStartAge && roll < 12)
        {
            return -1;
        }

        if (age <= PlayerDevelopmentConfig.YoungGrowthMaxAge && growthRoom >= 10 && roll >= 88)
        {
            return 1;
        }

        if (age <= PlayerDevelopmentConfig.SlowGrowthMaxAge && growthRoom >= 15 && roll >= 94)
        {
            return 1;
        }

        return 0;
    }

    private static int StableHash(string value)
    {
        unchecked
        {
            int hash = 23;
            string safeValue = value ?? "";
            for (int i = 0; i < safeValue.Length; i++)
            {
                hash = hash * 31 + safeValue[i];
            }

            return hash == int.MinValue ? 0 : Math.Abs(hash);
        }
    }

    private static int StableRange(string seed, int minInclusive, int maxInclusive)
    {
        if (maxInclusive <= minInclusive)
        {
            return minInclusive;
        }

        int range = maxInclusive - minInclusive + 1;
        return minInclusive + (StableHash(seed) % range);
    }

    private static int WeightedDelta(int roll, int veryLow, int low, int stableLow, int stableHigh, int high, int veryHigh)
    {
        if (roll < 10)
        {
            return veryLow;
        }

        if (roll < 25)
        {
            return low;
        }

        if (roll < 55)
        {
            return stableLow;
        }

        if (roll < 75)
        {
            return stableHigh;
        }

        if (roll < 92)
        {
            return high;
        }

        return veryHigh;
    }

    private static int ClampDelta(int value, int minValue, int maxValue)
    {
        if (value < minValue)
        {
            return minValue;
        }

        if (value > maxValue)
        {
            return maxValue;
        }

        return value;
    }

    private static PlayerDevelopmentChangeData CreateChange(
        GameState state,
        string entityType,
        string playerId,
        string playerName,
        string teamId,
        string teamName,
        string position,
        int age,
        int oldOverall,
        int newOverall,
        int oldPotential,
        int newPotential,
        string developmentType,
        string reason)
    {
        return new PlayerDevelopmentChangeData
        {
            ChangeId = Guid.NewGuid().ToString("N"),
            SeasonStartYear = state.CurrentSeasonStartYear,
            SeasonEndYear = state.CurrentSeasonEndYear,
            EntityType = entityType,
            PlayerId = playerId,
            PlayerName = playerName,
            TeamId = teamId,
            TeamName = teamName,
            Position = position,
            Age = age,
            OldOverall = oldOverall,
            NewOverall = newOverall,
            OverallDelta = newOverall - oldOverall,
            OldPotential = oldPotential,
            NewPotential = newPotential,
            PotentialDelta = newPotential - oldPotential,
            DevelopmentType = developmentType,
            Reason = reason,
            CreatedAtUtc = DateTime.UtcNow.ToString("o")
        };
    }

    private static void AddChange(GameState state, PlayerDevelopmentChangeData change)
    {
        if (state == null || change == null)
        {
            return;
        }

        EnsureDevelopmentHistory(state);
        state.PlayerDevelopmentHistory.Changes.Add(change);
    }

    private static bool IsAlreadyProcessed(string entityType, string id)
    {
        if (_processedDevelopmentKeys == null || string.IsNullOrEmpty(id))
        {
            return false;
        }

        return _processedDevelopmentKeys.Contains(GetProcessedKey(entityType, id));
    }

    private static void MarkProcessed(string entityType, string id)
    {
        if (_processedDevelopmentKeys != null && !string.IsNullOrEmpty(id))
        {
            _processedDevelopmentKeys.Add(GetProcessedKey(entityType, id));
        }
    }

    private static string GetProcessedKey(string entityType, string id)
    {
        string prefix = entityType == "DraftRightsProspect" ? "prospect:" : "player:";
        return prefix + id;
    }

    private static string GetDevelopmentType(int overallDelta)
    {
        if (overallDelta > 0)
        {
            return "Growth";
        }

        if (overallDelta < 0)
        {
            return "Regression";
        }

        return "Stable";
    }

    private static string GetReason(string entityType, int age, int overallDelta)
    {
        string baseReason;
        if (overallDelta > 0 && age <= PlayerDevelopmentConfig.SlowGrowthMaxAge)
        {
            baseReason = "Young player development";
        }
        else if (overallDelta < 0 && age >= PlayerDevelopmentConfig.VeteranRegressionStartAge)
        {
            baseReason = "Age-related regression";
        }
        else
        {
            baseReason = "No significant change";
        }

        if (entityType == "FreeAgent")
        {
            return baseReason + " | Free agent development";
        }

        if (entityType == "RosterPlayer")
        {
            return baseReason + " | Roster player development";
        }

        if (entityType == "DraftRightsProspect")
        {
            return baseReason + " | Draft rights prospect development";
        }

        return baseReason;
    }

    private static string GetTeamName(TeamData team)
    {
        return team == null ? "" : team.City + " " + team.Name;
    }
}
