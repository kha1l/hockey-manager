using System;
using System.Collections.Generic;

public static class TeamNeedService
{
    public static TeamNeedData CalculateTeamNeeds(GameState state, TeamData team)
    {
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        string direction = TeamDirectionService.DetermineTeamDirection(state, team);
        TeamNeedData needs = new TeamNeedData
        {
            TeamId = team == null ? "" : team.Id,
            TeamName = GetTeamName(team),
            Direction = direction,
            NeedTop6Forward = CalculateTop6ForwardNeed(team),
            NeedBottom6Forward = CalculateBottom6ForwardNeed(team),
            NeedDefenseman = CalculateDefenseNeed(team),
            NeedGoalie = CalculateGoalieNeed(team),
            NeedProspects = CalculateProspectNeed(team),
            NeedDraftPicks = CalculateDraftPickNeed(state, team),
            NeedCapSpace = CalculateCapSpaceNeed(state, team),
            NeedRosterSpace = CalculateRosterSpaceNeed(team),
            NeedYoungPlayers = CalculateYoungPlayersNeed(team),
            NeedVeteranHelp = CalculateVeteranHelpNeed(state, team, direction),
            UpdatedAtUtc = DateTime.UtcNow.ToString("o")
        };

        needs.PrimaryNeed = DeterminePrimaryNeed(needs);
        needs.SecondaryNeed = DetermineSecondaryNeed(needs);
        needs.OverallNeedScore = CalculateOverallNeedScore(needs);
        return needs;
    }

    public static void EnsureTeamNeeds(GameState state)
    {
        if (state == null)
        {
            return;
        }

        TeamTradeProfileService.EnsureTradeProfiles(state);
    }

    public static TeamNeedData GetTeamNeeds(GameState state, TeamData team)
    {
        TeamTradeProfileData profile = TeamTradeProfileService.GetTradeProfile(state, team == null ? "" : team.Id);
        if (profile != null && profile.Needs != null)
        {
            return profile.Needs;
        }

        return CalculateTeamNeeds(state, team);
    }

    public static string DeterminePrimaryNeed(TeamNeedData needs)
    {
        return GetNeedNameByRank(needs, 0);
    }

    public static string DetermineSecondaryNeed(TeamNeedData needs)
    {
        return GetNeedNameByRank(needs, 1);
    }

    public static int CalculateCapPressureScore(GameState state, TeamData team)
    {
        return CalculateCapSpaceNeed(state, team);
    }

    private static int CalculateTop6ForwardNeed(TeamData team)
    {
        int count = 0;
        int injured = 0;
        foreach (PlayerData player in GetOrganizationPlayers(team))
        {
            if (TradeAiConfig.IsForward(player) && player.Overall >= 78)
            {
                count++;
                if (!InjuryService.IsPlayerAvailable(player))
                {
                    injured++;
                }
            }
        }

        int need = Math.Max(0, 6 - count) * 16;
        need += injured * 8;
        return TradeAiConfig.ClampScore(need);
    }

    private static int CalculateBottom6ForwardNeed(TeamData team)
    {
        int count = 0;
        foreach (PlayerData player in GetOrganizationPlayers(team))
        {
            if (TradeAiConfig.IsForward(player) && player.Overall >= 68)
            {
                count++;
            }
        }

        return TradeAiConfig.ClampScore(Math.Max(0, 12 - count) * 9);
    }

    private static int CalculateDefenseNeed(TeamData team)
    {
        int topFour = 0;
        int healthyNhl = 0;
        foreach (PlayerData player in GetOrganizationPlayers(team))
        {
            if (!TradeAiConfig.IsDefenseman(player))
            {
                continue;
            }

            if (player.Overall >= 76)
            {
                topFour++;
            }

            if (RosterStatusConfig.IsNhlRoster(player) && InjuryService.IsPlayerAvailable(player))
            {
                healthyNhl++;
            }
        }

        if (healthyNhl < 6)
        {
            return TradeAiConfig.NeedCritical;
        }

        return TradeAiConfig.ClampScore(Math.Max(0, 4 - topFour) * 22);
    }

    private static int CalculateGoalieNeed(TeamData team)
    {
        List<PlayerData> goalies = new List<PlayerData>();
        foreach (PlayerData player in GetOrganizationPlayers(team))
        {
            if (TradeAiConfig.IsGoalie(player))
            {
                goalies.Add(player);
            }
        }

        if (goalies.Count < 2)
        {
            return TradeAiConfig.NeedCritical;
        }

        goalies.Sort(CompareOverallDescending);
        if (goalies[0].Overall < 78)
        {
            return TradeAiConfig.NeedHigh;
        }

        if (goalies.Count < 2 || goalies[1].Overall < 70)
        {
            return TradeAiConfig.NeedMedium;
        }

        return 0;
    }

    private static int CalculateProspectNeed(TeamData team)
    {
        int youngUpside = 0;
        foreach (PlayerData player in GetOrganizationPlayers(team))
        {
            if (player.Age <= 23 && player.Potential >= 78)
            {
                youngUpside++;
            }
        }

        int draftRightsCount = team == null || team.DraftRights == null ? 0 : team.DraftRights.Count;
        youngUpside += draftRightsCount / 2;
        return TradeAiConfig.ClampScore(Math.Max(0, 6 - youngUpside) * 14);
    }

    private static int CalculateDraftPickNeed(GameState state, TeamData team)
    {
        string direction = TeamDirectionService.DetermineTeamDirection(state, team);
        int need = direction == TradeAiConfig.DirectionRebuild ? 70 : direction == TradeAiConfig.DirectionRetool ? 55 : 15;
        int ownedPicks = 0;
        if (state != null && team != null)
        {
            List<DraftPickOwnershipData> picks = DraftPickOwnershipService.GetOwnedPicks(state, team.Id);
            ownedPicks = picks == null ? 0 : picks.Count;
        }

        if (ownedPicks <= 2)
        {
            need += 20;
        }
        else if (ownedPicks >= 5)
        {
            need -= 20;
        }

        return TradeAiConfig.ClampScore(need);
    }

    private static int CalculateCapSpaceNeed(GameState state, TeamData team)
    {
        if (team == null)
        {
            return 0;
        }

        int cap = state != null && state.LeagueRules != null && state.LeagueRules.SalaryCapUpperLimit > 0
            ? state.LeagueRules.SalaryCapUpperLimit
            : SalaryCapConfig.SalaryCapUpperLimit;
        int payroll = SalaryCapService.CalculatePayroll(team);
        if (payroll > cap)
        {
            return 100;
        }

        if (payroll > cap * 95 / 100)
        {
            return TradeAiConfig.NeedHigh;
        }

        if (payroll > cap * 90 / 100)
        {
            return TradeAiConfig.NeedMedium;
        }

        return 0;
    }

    private static int CalculateRosterSpaceNeed(TeamData team)
    {
        if (team == null)
        {
            return 0;
        }

        int nhlCount = TeamRosterService.GetNhlPlayers(team).Count;
        if (nhlCount > RosterStatusConfig.MaxNhlRosterSize)
        {
            return 100;
        }

        int lowOverallNhl = 0;
        foreach (PlayerData player in TeamRosterService.GetNhlPlayers(team))
        {
            if (player != null && player.Overall <= CpuRosterManagementConfig.LowOverallSendDownThreshold)
            {
                lowOverallNhl++;
            }
        }

        if (nhlCount == RosterStatusConfig.MaxNhlRosterSize && lowOverallNhl >= 3)
        {
            return TradeAiConfig.NeedHigh;
        }

        if (nhlCount == RosterStatusConfig.MaxNhlRosterSize)
        {
            return TradeAiConfig.NeedMedium;
        }

        return 0;
    }

    private static int CalculateYoungPlayersNeed(TeamData team)
    {
        int totalAge = 0;
        int total = 0;
        int young = 0;
        foreach (PlayerData player in GetOrganizationPlayers(team))
        {
            totalAge += player.Age;
            total++;
            if (TradeAiConfig.IsYoungPlayer(player) && player.Potential >= 76)
            {
                young++;
            }
        }

        int averageAge = total == 0 ? 0 : totalAge / total;
        int need = Math.Max(0, 7 - young) * 10;
        if (averageAge >= 29)
        {
            need += 25;
        }

        return TradeAiConfig.ClampScore(need);
    }

    private static int CalculateVeteranHelpNeed(GameState state, TeamData team, string direction)
    {
        if (direction != TradeAiConfig.DirectionContender && direction != TradeAiConfig.DirectionPlayoffTeam)
        {
            return 0;
        }

        int strongVeterans = 0;
        foreach (PlayerData player in GetOrganizationPlayers(team))
        {
            if (TradeAiConfig.IsVeteran(player) && player.Overall >= 80)
            {
                strongVeterans++;
            }
        }

        return TradeAiConfig.ClampScore(Math.Max(0, 5 - strongVeterans) * 14);
    }

    private static int CalculateOverallNeedScore(TeamNeedData needs)
    {
        if (needs == null)
        {
            return 0;
        }

        int max = 0;
        int total = 0;
        int count = 0;
        foreach (NeedEntry entry in BuildNeedEntries(needs))
        {
            max = Math.Max(max, entry.Score);
            total += entry.Score;
            count++;
        }

        int average = count == 0 ? 0 : total / count;
        return TradeAiConfig.ClampScore((max * 2 + average) / 3);
    }

    private static string GetNeedNameByRank(TeamNeedData needs, int rank)
    {
        List<NeedEntry> entries = BuildNeedEntries(needs);
        entries.Sort(CompareNeedEntriesDescending);
        if (rank < 0 || rank >= entries.Count || entries[rank].Score <= 0)
        {
            return "None";
        }

        return entries[rank].Name;
    }

    private static List<NeedEntry> BuildNeedEntries(TeamNeedData needs)
    {
        List<NeedEntry> entries = new List<NeedEntry>();
        if (needs == null)
        {
            return entries;
        }

        entries.Add(new NeedEntry("Top6Forward", needs.NeedTop6Forward));
        entries.Add(new NeedEntry("Bottom6Forward", needs.NeedBottom6Forward));
        entries.Add(new NeedEntry("Defenseman", needs.NeedDefenseman));
        entries.Add(new NeedEntry("Goalie", needs.NeedGoalie));
        entries.Add(new NeedEntry("Prospects", needs.NeedProspects));
        entries.Add(new NeedEntry("DraftPicks", needs.NeedDraftPicks));
        entries.Add(new NeedEntry("CapSpace", needs.NeedCapSpace));
        entries.Add(new NeedEntry("RosterSpace", needs.NeedRosterSpace));
        entries.Add(new NeedEntry("YoungPlayers", needs.NeedYoungPlayers));
        entries.Add(new NeedEntry("VeteranHelp", needs.NeedVeteranHelp));
        return entries;
    }

    private static List<PlayerData> GetOrganizationPlayers(TeamData team)
    {
        List<PlayerData> players = new List<PlayerData>();
        if (team == null)
        {
            return players;
        }

        team.EnsurePlayers();
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        foreach (PlayerData player in team.Players)
        {
            if (player != null && RosterStatusConfig.IsInOrganization(player))
            {
                players.Add(player);
            }
        }

        return players;
    }

    private static int CompareOverallDescending(PlayerData left, PlayerData right)
    {
        int comparison = right.Overall.CompareTo(left.Overall);
        return comparison != 0 ? comparison : string.Compare(left.Id, right.Id, StringComparison.Ordinal);
    }

    private static int CompareNeedEntriesDescending(NeedEntry left, NeedEntry right)
    {
        int comparison = right.Score.CompareTo(left.Score);
        return comparison != 0 ? comparison : string.Compare(left.Name, right.Name, StringComparison.Ordinal);
    }

    private class NeedEntry
    {
        public string Name;
        public int Score;

        public NeedEntry(string name, int score)
        {
            Name = name;
            Score = TradeAiConfig.ClampScore(score);
        }
    }

    private static string GetTeamName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }
}
