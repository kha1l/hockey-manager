using System.Collections.Generic;
using UnityEngine;

public static class PlayerExpectationService
{
    public static void EnsureExpectationsForTeam(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            EnsurePlayerExpectations(player);
        }
    }

    public static void EnsureExpectationsForTeams(List<TeamData> teams)
    {
        if (teams == null)
        {
            return;
        }

        foreach (TeamData team in teams)
        {
            EnsureExpectationsForTeam(team);
        }
    }

    public static void EnsurePlayerExpectations(PlayerData player)
    {
        if (player == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(player.ExpectedRole))
        {
            player.ExpectedRole = DetermineExpectedRole(player);
        }

        if (string.IsNullOrEmpty(player.ExpectedUsageCategory))
        {
            player.ExpectedUsageCategory = DetermineExpectedUsageCategory(player);
        }

        if (player.ExpectedTimeOnIceSeconds <= 0)
        {
            player.ExpectedTimeOnIceSeconds = DetermineExpectedTimeOnIceSeconds(player);
        }
    }

    public static string DetermineExpectedRole(PlayerData player)
    {
        if (player == null)
        {
            return "DepthForward";
        }

        if (player.Position == "G")
        {
            if (player.Overall >= 82)
            {
                return "StarterGoalie";
            }

            if (player.Overall >= 72)
            {
                return "BackupGoalie";
            }

            return player.Age <= 23 ? "GoalieProspect" : "DepthGoalie";
        }

        if (player.Position == "D")
        {
            if (player.Overall >= 84)
            {
                return "StarDefenseman";
            }

            if (player.Overall >= 79)
            {
                return "Top4Defenseman";
            }

            if (player.Overall >= 72)
            {
                return "ThirdPairDefenseman";
            }

            return player.Age <= 23 && player.Potential >= 80 ? "DefenseProspect" : "DepthDefenseman";
        }

        if (player.Overall >= 85)
        {
            return "StarForward";
        }

        if (player.Overall >= 80)
        {
            return "TopSixForward";
        }

        if (player.Overall >= 74)
        {
            return "MiddleSixForward";
        }

        if (player.Overall >= 68)
        {
            return "BottomSixForward";
        }

        return player.Age <= 23 && player.Potential >= 80 ? "ForwardProspect" : "DepthForward";
    }

    public static string DetermineExpectedUsageCategory(PlayerData player)
    {
        string expectedRole = string.IsNullOrEmpty(player == null ? "" : player.ExpectedRole)
            ? DetermineExpectedRole(player)
            : player.ExpectedRole;

        if (expectedRole == "StarForward")
        {
            return "FirstLine";
        }

        if (expectedRole == "TopSixForward")
        {
            return "SecondLine";
        }

        if (expectedRole == "MiddleSixForward")
        {
            return "ThirdLine";
        }

        if (expectedRole == "BottomSixForward")
        {
            return "FourthLine";
        }

        if (expectedRole == "ForwardProspect")
        {
            return player != null && player.Overall >= 72 ? "ThirdLine" : "Farm";
        }

        if (expectedRole == "StarDefenseman")
        {
            return "FirstPair";
        }

        if (expectedRole == "Top4Defenseman")
        {
            return "SecondPair";
        }

        if (expectedRole == "ThirdPairDefenseman")
        {
            return "ThirdPair";
        }

        if (expectedRole == "DefenseProspect")
        {
            return player != null && player.Overall >= 72 ? "ThirdPair" : "Farm";
        }

        if (expectedRole == "StarterGoalie")
        {
            return "StartingGoalie";
        }

        if (expectedRole == "BackupGoalie")
        {
            return "BackupGoalie";
        }

        if (expectedRole == "GoalieProspect")
        {
            return player != null && player.Overall >= 72 ? "BackupGoalie" : "Farm";
        }

        return "Scratch";
    }

    public static int DetermineExpectedTimeOnIceSeconds(PlayerData player)
    {
        string expectedRole = string.IsNullOrEmpty(player == null ? "" : player.ExpectedRole)
            ? DetermineExpectedRole(player)
            : player.ExpectedRole;

        if (expectedRole == "StarForward")
        {
            return 19 * 60;
        }

        if (expectedRole == "TopSixForward")
        {
            return 17 * 60;
        }

        if (expectedRole == "MiddleSixForward")
        {
            return 15 * 60;
        }

        if (expectedRole == "BottomSixForward")
        {
            return 10 * 60;
        }

        if (expectedRole == "StarDefenseman")
        {
            return 23 * 60;
        }

        if (expectedRole == "Top4Defenseman")
        {
            return 20 * 60;
        }

        if (expectedRole == "ThirdPairDefenseman")
        {
            return 16 * 60;
        }

        return expectedRole == "StarterGoalie" ? 3600 : 0;
    }

    public static bool IsRoleExpectationSatisfied(PlayerData player)
    {
        return CalculateRoleSatisfaction(player) >= 65;
    }

    public static int CalculateRoleSatisfaction(PlayerData player)
    {
        if (player == null)
        {
            return MoraleConfig.DefaultMorale;
        }

        if (player.IsInjured)
        {
            return Mathf.Max(65, player.RoleSatisfaction <= 0 ? 65 : player.RoleSatisfaction);
        }

        EnsurePlayerExpectations(player);
        int expected = GetUsageRank(player.ExpectedUsageCategory);
        int actual = GetUsageRank(string.IsNullOrEmpty(player.UsageCategory) ? "Scratch" : player.UsageCategory);

        if ((player.ExpectedUsageCategory == "Farm" && (RosterStatusConfig.IsFarmRoster(player) || RosterStatusConfig.IsReserve(player)))
            || actual <= expected)
        {
            return 88;
        }

        int gap = actual - expected;
        if ((player.ExpectedRole == "ForwardProspect" || player.ExpectedRole == "DefenseProspect" || player.ExpectedRole == "GoalieProspect")
            && RosterStatusConfig.IsFarmRoster(player))
        {
            return 68;
        }

        if (gap == 1)
        {
            return 68;
        }

        if (RosterStatusConfig.IsFarmRoster(player) || RosterStatusConfig.IsReserve(player))
        {
            return IsHighExpectation(player) ? 22 : 48;
        }

        return gap >= 2 ? 42 : 60;
    }

    public static int CalculateIceTimeSatisfaction(PlayerData player)
    {
        if (player == null)
        {
            return MoraleConfig.DefaultMorale;
        }

        if (player.IsInjured)
        {
            return 70;
        }

        EnsurePlayerExpectations(player);
        int expected = player.ExpectedTimeOnIceSeconds;
        int actual = player.AverageTimeOnIceSeconds > 0
            ? player.AverageTimeOnIceSeconds
            : player.EstimatedTimeOnIceSeconds;

        if (expected <= 0)
        {
            return actual == 0 ? 75 : 85;
        }

        float ratio = (float)actual / expected;
        if (ratio >= 1f)
        {
            return 90;
        }

        if (ratio >= 0.85f)
        {
            return 80;
        }

        if (ratio >= 0.65f)
        {
            return 62;
        }

        if (ratio >= 0.40f)
        {
            return 45;
        }

        return 28;
    }

    private static bool IsHighExpectation(PlayerData player)
    {
        return player != null
            && (player.ExpectedRole == "StarForward"
                || player.ExpectedRole == "TopSixForward"
                || player.ExpectedRole == "StarDefenseman"
                || player.ExpectedRole == "Top4Defenseman"
                || player.ExpectedRole == "StarterGoalie"
                || player.ExpectedRole == "BackupGoalie");
    }

    private static int GetUsageRank(string usageCategory)
    {
        if (usageCategory == "FirstLine" || usageCategory == "FirstPair" || usageCategory == "StartingGoalie")
        {
            return 1;
        }

        if (usageCategory == "SecondLine" || usageCategory == "SecondPair" || usageCategory == "BackupGoalie")
        {
            return 2;
        }

        if (usageCategory == "ThirdLine" || usageCategory == "ThirdPair")
        {
            return 3;
        }

        if (usageCategory == "FourthLine")
        {
            return 4;
        }

        if (usageCategory == "Scratch")
        {
            return 5;
        }

        return usageCategory == "Farm" || usageCategory == "Reserve" ? 6 : 5;
    }
}
