public static class ContractExtensionInterestService
{
    public static int CalculateExtensionInterest(GameState state, TeamData team, PlayerData player)
    {
        if (player == null)
        {
            return 0;
        }

        MoraleService.InitializePlayerMorale(player);
        PlayerRoleService.EnsureRole(player);
        LeadershipService.EnsurePlayerLeadershipProfile(player);

        int interest = 55;
        interest += GetMoraleInterestModifier(player);
        interest += GetRoleInterestModifier(player);
        interest += GetRosterStatusInterestModifier(player);
        interest += GetTeamPerformanceInterestModifier(state, team, player);
        interest += GetLeadershipInterestModifier(team, player);
        interest += GetStaffInterestModifier(team, player);
        interest += GetTradeRequestInterestModifier(player);

        return ContractExtensionConfig.ClampInterest(interest);
    }

    public static string BuildInterestSummary(GameState state, TeamData team, PlayerData player, int interest)
    {
        if (player == null)
        {
            return "Нет данных";
        }

        if (player.RefusesExtensionThisSeason)
        {
            return "Игрок отказывается вести переговоры в этом сезоне";
        }

        if (player.WantsTrade || interest < ContractExtensionConfig.LowInterestThreshold)
        {
            return "Низкий интерес: плохая morale, статус или игрок хочет обмен";
        }

        if (interest >= ContractExtensionConfig.HighInterestThreshold)
        {
            return "Высокий интерес: игрок доволен ролью и ситуацией в клубе";
        }

        if (interest >= ContractExtensionConfig.MediumInterestThreshold)
        {
            return "Средний интерес: роль приемлемая, но условия контракта важны";
        }

        return "Низкий интерес: игроку нужны сильные условия для продления";
    }

    private static int GetMoraleInterestModifier(PlayerData player)
    {
        if (player.Morale >= 85)
        {
            return 20;
        }

        if (player.Morale >= 70)
        {
            return 10;
        }

        if (player.Morale >= 50)
        {
            return 0;
        }

        return player.Morale >= 35 ? -15 : -30;
    }

    private static int GetRoleInterestModifier(PlayerData player)
    {
        int modifier = 0;
        if (player.RoleSatisfaction >= 80)
        {
            modifier += 10;
        }
        else if (player.RoleSatisfaction < 40)
        {
            modifier -= 15;
        }

        if (player.IceTimeSatisfaction >= 80)
        {
            modifier += 8;
        }
        else if (player.IceTimeSatisfaction < 40)
        {
            modifier -= 12;
        }

        return modifier;
    }

    private static int GetRosterStatusInterestModifier(PlayerData player)
    {
        if (RosterStatusConfig.IsNhlRoster(player))
        {
            return 5;
        }

        if (RosterStatusConfig.IsFarmRoster(player))
        {
            if (player.Age <= 23 && player.Overall < 74)
            {
                return -5;
            }

            return player.Overall >= 78 || player.Age >= 28 ? -25 : -10;
        }

        return RosterStatusConfig.IsReserve(player) ? -20 : 0;
    }

    private static int GetTeamPerformanceInterestModifier(GameState state, TeamData team, PlayerData player)
    {
        TeamStandingData standing = FindStanding(state, team);
        if (standing == null || standing.GamesPlayed <= 0)
        {
            return 0;
        }

        int pointsPercentage = standing.Points * 100 / (standing.GamesPlayed * 2);
        if (pointsPercentage >= 60)
        {
            return player.Age >= 28 || player.Overall >= 82 ? 10 : 4;
        }

        if (pointsPercentage < 45)
        {
            return player.Age <= 23 ? -3 : -10;
        }

        return 0;
    }

    private static int GetLeadershipInterestModifier(TeamData team, PlayerData player)
    {
        int modifier = 0;
        if ((player.IsCaptain || player.IsAlternateCaptain) && player.Morale >= 60)
        {
            modifier += player.IsCaptain ? 8 : 5;
        }

        int leadershipImpact = LeadershipService.GetTeamMoraleImpact(team);
        if (leadershipImpact > 0)
        {
            modifier += 3;
        }
        else if (leadershipImpact < 0)
        {
            modifier -= 3;
        }

        return modifier;
    }

    private static int GetStaffInterestModifier(TeamData team, PlayerData player)
    {
        int moraleModifier = CoachingStaffService.GetMoraleModifier(team);
        if (moraleModifier > 0)
        {
            return 3;
        }

        return moraleModifier < 0 ? -3 : 0;
    }

    private static int GetTradeRequestInterestModifier(PlayerData player)
    {
        return player != null && player.WantsTrade ? -40 : 0;
    }

    private static TeamStandingData FindStanding(GameState state, TeamData team)
    {
        if (state == null || state.Season == null || state.Season.Standings == null || team == null)
        {
            return null;
        }

        foreach (TeamStandingData standing in state.Season.Standings)
        {
            if (standing != null && standing.TeamId == team.Id)
            {
                return standing;
            }
        }

        return null;
    }
}
