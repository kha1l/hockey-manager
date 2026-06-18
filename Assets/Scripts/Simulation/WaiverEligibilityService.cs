using System.Collections.Generic;

public static class WaiverEligibilityService
{
    public static void EnsureWaiverEligibilityForTeam(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            EnsureWaiverEligibility(player);
        }
    }

    public static void EnsureWaiverEligibilityForTeams(List<TeamData> teams)
    {
        if (teams == null)
        {
            return;
        }

        foreach (TeamData team in teams)
        {
            EnsureWaiverEligibilityForTeam(team);
        }
    }

    public static void EnsureWaiverEligibility(PlayerData player)
    {
        EnsureWaiverFields(player);
        if (player == null)
        {
            return;
        }

        // TODO: Future: implement detailed waiver exemption rules by age, signing age and pro games played.
        player.IsWaiverEligible = false;
        player.RequiresWaivers = false;

        if (!RosterStatusConfig.IsNhlRoster(player))
        {
            return;
        }

        if (player.ContractYearsRemaining <= 0)
        {
            return;
        }

        if (player.IsEntryLevelContract && player.Age <= 22)
        {
            return;
        }

        bool goalieRequiresWaivers = player.Position == "G"
            && player.Age >= 24
            && player.Overall >= 68;
        bool skaterRequiresWaivers = player.Position != "G"
            && player.Age >= 23
            && player.Overall >= 70;

        if (goalieRequiresWaivers || skaterRequiresWaivers)
        {
            player.IsWaiverEligible = true;
            player.RequiresWaivers = true;
        }
    }

    public static bool RequiresWaiversForDemotion(PlayerData player)
    {
        EnsureWaiverEligibility(player);
        return player != null && player.RequiresWaivers;
    }

    public static string GetWaiverEligibilityReason(PlayerData player)
    {
        if (player == null)
        {
            return "Waivers: игрок не найден";
        }

        EnsureWaiverEligibility(player);

        if (!RosterStatusConfig.IsNhlRoster(player))
        {
            return "Не требует waivers: игрок не в Pro roster";
        }

        if (player.ContractYearsRemaining <= 0)
        {
            return "Не требует waivers: контракт не активен";
        }

        if (player.IsEntryLevelContract && player.Age <= 22)
        {
            return "Не требует waivers: молодой ELC игрок";
        }

        if (player.Position == "G")
        {
            if (player.Age < 24)
            {
                return "Не требует waivers: вратарь моложе 24";
            }

            if (player.Overall < 68)
            {
                return "Не требует waivers: низкий overall";
            }
        }
        else
        {
            if (player.Age < 23)
            {
                return "Не требует waivers: игрок моложе 23";
            }

            if (player.Overall < 70)
            {
                return "Не требует waivers: низкий overall";
            }
        }

        return "Требует waivers: возраст/контракт/уровень игрока";
    }

    public static void EnsureWaiverFields(PlayerData player)
    {
        if (player == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(player.WaiverStatus))
        {
            player.WaiverStatus = player.IsOnWaivers
                ? WaiverConfig.WaiverStatusOnWaivers
                : WaiverConfig.WaiverStatusNone;
        }

        if (!player.IsOnWaivers && player.WaiverStatus == WaiverConfig.WaiverStatusOnWaivers)
        {
            player.WaiverStatus = WaiverConfig.WaiverStatusNone;
        }

        if (player.WaiverPlacedAtUtc == null)
        {
            player.WaiverPlacedAtUtc = "";
        }

        if (player.WaiverExpiresAtUtc == null)
        {
            player.WaiverExpiresAtUtc = "";
        }

        if (player.WaiverOriginalTeamId == null)
        {
            player.WaiverOriginalTeamId = "";
        }

        if (player.WaiverOriginalTeamName == null)
        {
            player.WaiverOriginalTeamName = "";
        }

        if (player.WaiverIntendedDestination == null)
        {
            player.WaiverIntendedDestination = "";
        }
    }
}
