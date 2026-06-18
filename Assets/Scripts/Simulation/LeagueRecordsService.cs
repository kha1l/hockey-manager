using System;
using System.Collections.Generic;

public static class LeagueRecordsService
{
    public static void EnsureLeagueRecords(GameState state)
    {
        if (state == null)
        {
            return;
        }

        state.EnsureLeagueHistory();
        if (state.LeagueRecords == null)
        {
            state.LeagueRecords = new LeagueRecordsData();
        }

        state.LeagueRecords.EnsureRecords();
    }

    public static void UpdateRecordsAfterSeason(GameState state)
    {
        EnsureLeagueRecords(state);
        if (state == null || state.Season == null || state.Season.PlayerStats == null)
        {
            return;
        }

        foreach (PlayerSeasonStatsData stats in state.Season.PlayerStats)
        {
            if (stats == null)
            {
                continue;
            }

            PlayerData player = FindPlayerById(state, stats.PlayerId);
            TeamData team = FindTeamById(state, stats.TeamId) ?? FindTeamByPlayer(state, stats.PlayerId);
            ConsiderSeasonRecord(state, "MostGoalsSeason", "Most Goals in a Season", player, team, stats.Goals, stats.Goals + " goals");
            ConsiderSeasonRecord(state, "MostAssistsSeason", "Most Assists in a Season", player, team, stats.Assists, stats.Assists + " assists");
            ConsiderSeasonRecord(state, "MostPointsSeason", "Most Points in a Season", player, team, stats.Points, stats.Points + " points");
            ConsiderSeasonRecord(state, "MostWinsSeason", "Most Wins in a Season", player, team, stats.GoalieWins, stats.GoalieWins + " wins");
            ConsiderSeasonRecord(state, "MostShutoutsSeason", "Most Shutouts in a Season", player, team, stats.Shutouts, stats.Shutouts + " shutouts");
        }

        foreach (PlayerData player in GetAllPlayers(state))
        {
            TeamData team = FindTeamByPlayer(state, player == null ? "" : player.Id);
            ConsiderCareerRecord(state, "MostCareerGoals", "Most Career Goals", player, team, player == null ? 0 : player.CareerGoals, (player == null ? 0 : player.CareerGoals) + " goals");
            ConsiderCareerRecord(state, "MostCareerAssists", "Most Career Assists", player, team, player == null ? 0 : player.CareerAssists, (player == null ? 0 : player.CareerAssists) + " assists");
            ConsiderCareerRecord(state, "MostCareerPoints", "Most Career Points", player, team, player == null ? 0 : player.CareerPoints, (player == null ? 0 : player.CareerPoints) + " points");
            ConsiderCareerRecord(state, "MostCareerWins", "Most Career Wins", player, team, player == null ? 0 : player.CareerWins, (player == null ? 0 : player.CareerWins) + " wins");
            ConsiderCareerRecord(state, "MostCareerShutouts", "Most Career Shutouts", player, team, player == null ? 0 : player.CareerShutouts, (player == null ? 0 : player.CareerShutouts) + " shutouts");
        }

        state.LeagueRecords.UpdatedAtUtc = DateTime.UtcNow.ToString("o");
    }

    public static void ConsiderSeasonRecord(
        GameState state,
        string recordType,
        string recordName,
        PlayerData player,
        TeamData team,
        int value,
        string valueLabel)
    {
        ConsiderRecord(state, recordType, recordName, player, team, value, valueLabel, false);
    }

    public static void ConsiderCareerRecord(
        GameState state,
        string recordType,
        string recordName,
        PlayerData player,
        TeamData team,
        int value,
        string valueLabel)
    {
        ConsiderRecord(state, recordType, recordName, player, team, value, valueLabel, true);
    }

    public static LeagueRecordData GetRecord(GameState state, string recordType)
    {
        EnsureLeagueRecords(state);
        if (state == null || state.LeagueRecords == null || state.LeagueRecords.Records == null)
        {
            return null;
        }

        foreach (LeagueRecordData record in state.LeagueRecords.Records)
        {
            if (record != null && record.RecordType == recordType)
            {
                return record;
            }
        }

        return null;
    }

    public static void SetRecord(GameState state, LeagueRecordData record)
    {
        EnsureLeagueRecords(state);
        if (state == null || record == null)
        {
            return;
        }

        for (int i = state.LeagueRecords.Records.Count - 1; i >= 0; i--)
        {
            LeagueRecordData existing = state.LeagueRecords.Records[i];
            if (existing != null && existing.RecordType == record.RecordType)
            {
                state.LeagueRecords.Records.RemoveAt(i);
            }
        }

        state.LeagueRecords.Records.Add(record);
        state.LeagueRecords.UpdatedAtUtc = DateTime.UtcNow.ToString("o");
    }

    public static List<PlayerData> GetAllPlayers(GameState state)
    {
        return CareerStatsService.GetAllPlayersIncludingFreeAgents(state);
    }

    private static void ConsiderRecord(
        GameState state,
        string recordType,
        string recordName,
        PlayerData player,
        TeamData team,
        int value,
        string valueLabel,
        bool isCareerRecord)
    {
        if (state == null || player == null || value <= 0)
        {
            return;
        }

        LeagueRecordData existing = GetRecord(state, recordType);
        if (existing != null && existing.Value >= value)
        {
            return;
        }

        SetRecord(state, new LeagueRecordData
        {
            RecordId = recordType,
            RecordType = recordType,
            RecordName = recordName,
            PlayerId = player.Id,
            PlayerName = player.FirstName + " " + player.LastName,
            TeamId = team == null ? player.TeamId : team.Id,
            TeamName = TeamIdentityService.GetDisplayName(team),
            SeasonStartYear = state.CurrentSeasonStartYear,
            SeasonEndYear = state.CurrentSeasonEndYear,
            Value = value,
            ValueLabel = valueLabel,
            IsCareerRecord = isCareerRecord,
            CreatedAtUtc = DateTime.UtcNow.ToString("o")
        });
    }

    private static PlayerData FindPlayerById(GameState state, string playerId)
    {
        foreach (PlayerData player in GetAllPlayers(state))
        {
            if (player != null && player.Id == playerId)
            {
                return player;
            }
        }

        return null;
    }

    private static TeamData FindTeamByPlayer(GameState state, string playerId)
    {
        if (state == null || state.Teams == null || string.IsNullOrEmpty(playerId))
        {
            return null;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team == null)
            {
                continue;
            }

            team.EnsurePlayers();
            foreach (PlayerData player in team.Players)
            {
                if (player != null && player.Id == playerId)
                {
                    return team;
                }
            }
        }

        return null;
    }

    private static TeamData FindTeamById(GameState state, string teamId)
    {
        if (state == null || state.Teams == null || string.IsNullOrEmpty(teamId))
        {
            return null;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team != null && team.Id == teamId)
            {
                return team;
            }
        }

        return null;
    }
}
