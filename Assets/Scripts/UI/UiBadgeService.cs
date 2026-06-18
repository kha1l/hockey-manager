using System.Collections.Generic;

public static class UiBadgeService
{
    public static List<UiBadgeData> BuildPlayerBadges(PlayerData player)
    {
        List<UiBadgeData> badges = new List<UiBadgeData>();
        if (player == null)
        {
            return badges;
        }

        LeadershipService.EnsurePlayerLeadershipProfile(player);
        PlayerFatigueService.EnsureFatigueFields(player);
        InjuryService.EnsureInjuryFields(player);
        MoraleService.InitializePlayerMorale(player);

        if (player.IsRetired)
        {
            badges.Add(CreateBadge("RET", "History", 110));
        }

        if (player.IsHallOfFameInducted)
        {
            badges.Add(CreateBadge("HOF", "History", 108));
        }

        if (player.HasRetiredNumber)
        {
            badges.Add(CreateBadge("# RET", "History", 106));
        }

        if (player.IsCaptain)
        {
            badges.Add(CreateBadge("C", "Captaincy", 100));
        }
        else if (player.IsAlternateCaptain)
        {
            badges.Add(CreateBadge("A", "Captaincy", 95));
        }

        if (player.RosterStatus == RosterStatusConfig.NHL)
        {
            badges.Add(CreateBadge("Pro", "Roster", 80));
        }
        else if (player.RosterStatus == RosterStatusConfig.Farm)
        {
            badges.Add(CreateBadge("FARM", "Roster", 70));
        }
        else if (player.RosterStatus == RosterStatusConfig.Reserve)
        {
            badges.Add(CreateBadge("RES", "Roster", 65));
        }

        if (player.IsInjured)
        {
            badges.Add(CreateBadge("INJ " + player.InjuryDaysRemaining + "d", "Injury", 100));
        }

        if (player.WantsTrade)
        {
            badges.Add(CreateBadge("TRADE REQ", "Morale", 95));
        }

        if (player.Morale > 0 && player.Morale < 40)
        {
            badges.Add(CreateBadge("LOW MOR", "Morale", 85));
        }

        if (player.Condition > 0 && player.Condition < 70)
        {
            badges.Add(CreateBadge("TIRED", "Warning", 75));
        }

        if (player.ContractYearsRemaining <= 1)
        {
            badges.Add(CreateBadge("EXP", "Contract", 70));
        }

        if (player.IsEntryLevelContract)
        {
            badges.Add(CreateBadge("ELC", "Contract", 60));
        }

        if (player.IsOnWaivers)
        {
            badges.Add(CreateBadge("WAIVERS", "Roster", 90));
        }

        if (player.RefusesExtensionThisSeason)
        {
            badges.Add(CreateBadge("NO EXT", "Contract", 80));
        }

        badges.Sort(CompareByPriority);
        return badges;
    }

    public static List<UiBadgeData> BuildTeamBadges(GameState state, TeamData team)
    {
        List<UiBadgeData> badges = new List<UiBadgeData>();
        if (team == null)
        {
            return badges;
        }

        TeamRosterService.EnsureRosterStatusesForTeam(team);
        if (!TeamRosterService.ValidateNhlRoster(team, out string rosterMessage))
        {
            badges.Add(CreateBadge("ROSTER!", "Bad", 100));
        }

        LineupService.ValidateLineup(team, out string lineupMessage);
        if (team.Lineup == null || !team.Lineup.IsValid)
        {
            badges.Add(CreateBadge("LINEUP!", "Bad", 95));
        }

        TeamFinanceData finance = SalaryCapService.CalculateTeamFinance(team);
        if (finance != null && finance.IsOverCap)
        {
            badges.Add(CreateBadge("OVER CAP", "Bad", 90));
        }
        else if (finance != null && finance.CapSpace < 2000000)
        {
            badges.Add(CreateBadge("CAP LOW", "Warning", 70));
        }

        TeamLeadershipData leadership = team.LeadershipData;
        if (leadership == null || string.IsNullOrEmpty(leadership.CaptainPlayerId))
        {
            badges.Add(CreateBadge("NO C", "Captaincy", 55));
        }

        team.EnsurePlayers();
        int injured = 0;
        int tradeRequests = 0;
        int expiring = 0;
        foreach (PlayerData player in team.Players)
        {
            if (player == null)
            {
                continue;
            }

            if (player.IsInjured)
            {
                injured++;
            }

            if (player.WantsTrade)
            {
                tradeRequests++;
            }

            if (player.ContractYearsRemaining <= 1)
            {
                expiring++;
            }
        }

        if (tradeRequests > 0)
        {
            badges.Add(CreateBadge("TRADE REQ", "Morale", 75));
        }

        if (injured > 0)
        {
            badges.Add(CreateBadge("INJ", "Injury", 70));
        }

        if (expiring > 0)
        {
            badges.Add(CreateBadge("EXP CONTRACTS", "Contract", 65));
        }

        OwnerProfileData owner = OwnerGoalService.GetOwnerProfile(state, team);
        if (owner != null && owner.GmTrust < 45)
        {
            badges.Add(CreateBadge("OWNER PRESSURE", "Owner", 65));
        }

        badges.Sort(CompareByPriority);
        return badges;
    }

    public static UiBadgeData CreateBadge(string text, string category, int priority)
    {
        return new UiBadgeData
        {
            Text = string.IsNullOrEmpty(text) ? "" : text,
            Category = string.IsNullOrEmpty(category) ? "Info" : category,
            Priority = priority
        };
    }

    public static string FormatBadgesInline(List<UiBadgeData> badges, int maxBadges)
    {
        if (badges == null || badges.Count == 0 || maxBadges == 0)
        {
            return "";
        }

        string text = "";
        int shown = 0;
        foreach (UiBadgeData badge in badges)
        {
            if (badge == null || string.IsNullOrEmpty(badge.Text))
            {
                continue;
            }

            if (maxBadges > 0 && shown >= maxBadges)
            {
                break;
            }

            if (shown > 0)
            {
                text += " ";
            }

            text += "[" + badge.Text + "]";
            shown++;
        }

        return text;
    }

    private static int CompareByPriority(UiBadgeData left, UiBadgeData right)
    {
        int leftPriority = left == null ? 0 : left.Priority;
        int rightPriority = right == null ? 0 : right.Priority;
        return rightPriority.CompareTo(leftPriority);
    }
}
