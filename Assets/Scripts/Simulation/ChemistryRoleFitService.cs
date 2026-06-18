using System.Collections.Generic;

public static class ChemistryRoleFitService
{
    public static int CalculateForwardLineRoleBalance(List<PlayerData> players)
    {
        EnsureRoles(players);
        int score = 60;
        if (HasRole(players, PlayerRoleConfig.Playmaker) && HasRole(players, PlayerRoleConfig.Sniper))
        {
            score += 12;
        }

        if (HasRole(players, PlayerRoleConfig.Playmaker) && HasRole(players, PlayerRoleConfig.PowerForward))
        {
            score += 8;
        }

        if (HasRole(players, PlayerRoleConfig.Sniper) && HasRole(players, PlayerRoleConfig.PowerForward))
        {
            score += 8;
        }

        if (HasRole(players, PlayerRoleConfig.TwoWayForward))
        {
            score += 6;
        }

        if (HasRole(players, PlayerRoleConfig.Grinder))
        {
            score += 3;
        }

        if (CountRoles(players, PlayerRoleConfig.Sniper) >= 3)
        {
            score -= 15;
        }

        if (CountRoles(players, PlayerRoleConfig.Playmaker) >= 3)
        {
            score -= 10;
        }

        if (CountRoles(players, PlayerRoleConfig.Grinder, PlayerRoleConfig.DepthForward) >= 3)
        {
            score -= 12;
        }

        if (!HasRole(players, PlayerRoleConfig.TwoWayForward) && !HasRole(players, PlayerRoleConfig.Grinder))
        {
            score -= 6;
        }

        return ChemistryConfig.ClampChemistry(score);
    }

    public static int CalculateDefensePairRoleBalance(List<PlayerData> players)
    {
        EnsureRoles(players);
        int score = 60;
        if (HasRole(players, PlayerRoleConfig.OffensiveDefenseman) && HasRole(players, PlayerRoleConfig.DefensiveDefenseman))
        {
            score += 15;
        }

        if (HasRole(players, PlayerRoleConfig.OffensiveDefenseman) && HasRole(players, PlayerRoleConfig.StayAtHomeDefenseman))
        {
            score += 12;
        }

        if (HasRole(players, PlayerRoleConfig.TwoWayDefenseman))
        {
            score += 8;
        }

        if (CountRoles(players, PlayerRoleConfig.OffensiveDefenseman) >= 2)
        {
            score -= 12;
        }

        if (CountRoles(players, PlayerRoleConfig.StayAtHomeDefenseman) >= 2)
        {
            score -= 8;
        }

        if (CountRoles(players, PlayerRoleConfig.DefensiveDefenseman, PlayerRoleConfig.StayAtHomeDefenseman) >= 2
            && !HasRole(players, PlayerRoleConfig.TwoWayDefenseman))
        {
            score -= 8;
        }

        return ChemistryConfig.ClampChemistry(score);
    }

    public static int CalculatePowerPlayRoleBalance(List<PlayerData> players)
    {
        EnsureRoles(players);
        int score = 60;
        score += CountRoles(players, PlayerRoleConfig.Sniper) * 8;
        score += CountRoles(players, PlayerRoleConfig.Playmaker) * 8;
        score += CountRoles(players, PlayerRoleConfig.OffensiveDefenseman) * 8;
        score += CountRoles(players, PlayerRoleConfig.PowerForward) * 5;

        if (CountRoles(players, PlayerRoleConfig.Grinder, PlayerRoleConfig.DepthForward, PlayerRoleConfig.StayAtHomeDefenseman) >= 3)
        {
            score -= 10;
        }

        return ChemistryConfig.ClampChemistry(score);
    }

    public static int CalculatePenaltyKillRoleBalance(List<PlayerData> players)
    {
        EnsureRoles(players);
        int score = 60;
        score += CountRoles(players, PlayerRoleConfig.TwoWayForward) * 10;
        score += CountRoles(players, PlayerRoleConfig.Grinder) * 8;
        score += CountRoles(players, PlayerRoleConfig.DefensiveDefenseman) * 10;
        score += CountRoles(players, PlayerRoleConfig.StayAtHomeDefenseman) * 8;
        score += CountRoles(players, PlayerRoleConfig.TwoWayDefenseman) * 8;

        if (CountRoles(players, PlayerRoleConfig.Sniper, PlayerRoleConfig.OffensiveDefenseman) >= 3
            && CountRoles(players, PlayerRoleConfig.TwoWayForward, PlayerRoleConfig.Grinder, PlayerRoleConfig.DefensiveDefenseman, PlayerRoleConfig.StayAtHomeDefenseman, PlayerRoleConfig.TwoWayDefenseman) == 0)
        {
            score -= 10;
        }

        return ChemistryConfig.ClampChemistry(score);
    }

    public static int CalculatePositionFit(List<PlayerData> players, string unitType)
    {
        players = players ?? new List<PlayerData>();
        if (unitType == "ForwardLine")
        {
            int centers = CountPositions(players, "C");
            int leftWings = CountPositions(players, "LW");
            int rightWings = CountPositions(players, "RW");
            int score = 60;
            if (centers >= 1 && leftWings + rightWings >= 2)
            {
                score += 20;
            }
            else if (centers == 0)
            {
                score -= 20;
            }

            if (leftWings >= 3 || rightWings >= 3)
            {
                score -= 10;
            }

            return ChemistryConfig.ClampChemistry(score);
        }

        if (unitType == "DefensePair")
        {
            return CountPositions(players, "D") == 2 ? 85 : 25;
        }

        if (unitType == "PowerPlay")
        {
            int defensemen = CountPositions(players, "D");
            return ChemistryConfig.ClampChemistry(defensemen == 0 ? 50 : 70);
        }

        if (unitType == "PenaltyKill")
        {
            int forwards = CountPositions(players, "C") + CountPositions(players, "LW") + CountPositions(players, "RW");
            return ChemistryConfig.ClampChemistry(forwards == 0 ? 45 : 70);
        }

        return ChemistryConfig.DefaultChemistry;
    }

    private static bool HasRole(List<PlayerData> players, string role)
    {
        return CountRoles(players, role) > 0;
    }

    private static int CountRoles(List<PlayerData> players, params string[] roles)
    {
        if (players == null || roles == null)
        {
            return 0;
        }

        int count = 0;
        foreach (PlayerData player in players)
        {
            if (player == null)
            {
                continue;
            }

            PlayerRoleService.EnsureRole(player);
            foreach (string role in roles)
            {
                if (player.PlayerRole == role)
                {
                    count++;
                    break;
                }
            }
        }

        return count;
    }

    private static void EnsureRoles(List<PlayerData> players)
    {
        if (players == null)
        {
            return;
        }

        foreach (PlayerData player in players)
        {
            PlayerRoleService.EnsureRole(player);
        }
    }

    private static int CountPositions(List<PlayerData> players, string position)
    {
        if (players == null)
        {
            return 0;
        }

        int count = 0;
        foreach (PlayerData player in players)
        {
            if (player != null && player.Position == position)
            {
                count++;
            }
        }

        return count;
    }
}
