using System.Collections.Generic;

public static class PlayerDisplayFormatter
{
    public static string FormatPlayerMainLine(PlayerData player)
    {
        if (player == null)
        {
            return "Игрок не найден";
        }

        return FormatJerseyNumber(player) + GetPlayerName(player)
            + " | " + SafeText(player.Position)
            + " | " + SafeText(player.Nationality)
            + " | " + MobileUiConfig.FormatOverall(player.Overall)
            + " | POT " + player.Potential
            + " | AGE " + player.Age;
    }

    public static string FormatPlayerSubLine(PlayerData player)
    {
        if (player == null)
        {
            return "";
        }

        EnsurePlayerUiFields(player);
        string badges = UiBadgeService.FormatBadgesInline(UiBadgeService.BuildPlayerBadges(player), 5);
        string role = string.IsNullOrEmpty(player.PlayerRole) ? "Role n/a" : player.PlayerRole;
        return badges
            + (string.IsNullOrEmpty(badges) ? "" : " ")
            + role
            + " | " + MobileUiConfig.FormatMoney(player.Salary)
            + " | " + player.ContractYearsRemaining + "y"
            + " | MOR " + player.Morale
            + " | COND " + player.Condition;
    }

    public static string FormatPlayerCompact(PlayerData player)
    {
        if (player == null)
        {
            return "Игрок не найден";
        }

        return FormatJerseyNumber(player) + GetPlayerName(player)
            + " | " + SafeText(player.Position)
            + " | " + SafeText(player.Nationality)
            + " | " + MobileUiConfig.FormatOverall(player.Overall)
            + " | " + UiBadgeService.FormatBadgesInline(UiBadgeService.BuildPlayerBadges(player), 3);
    }

    public static string FormatPlayerWithContract(PlayerData player)
    {
        if (player == null)
        {
            return "Контракт недоступен";
        }

        EnsurePlayerUiFields(player);
        string extension = player.IsExtensionEligible
            ? " | EXT INT " + player.ExtensionInterest
            : "";
        if (player.RefusesExtensionThisSeason)
        {
            extension += " | NO EXT";
        }

        return FormatPlayerMainLine(player)
            + "\n" + UiBadgeService.FormatBadgesInline(UiBadgeService.BuildPlayerBadges(player), 6)
            + " | " + MobileUiConfig.FormatMoney(player.Salary)
            + " | " + player.ContractYearsRemaining + "y"
            + " | " + MobileUiConfig.FormatShortStatus(player.ContractStatus)
            + extension;
    }

    public static string FormatPlayerWithMorale(PlayerData player)
    {
        if (player == null)
        {
            return "Мораль недоступна";
        }

        EnsurePlayerUiFields(player);
        return FormatPlayerCompact(player)
            + "\nMOR " + player.Morale
            + " | " + MobileUiConfig.FormatShortStatus(player.MoraleStatus)
            + (player.WantsTrade ? " | TRADE REQ" : "")
            + " | " + MobileUiConfig.FormatShortStatus(player.MoraleSummary);
    }

    public static string FormatPlayerWithUsage(PlayerData player)
    {
        if (player == null)
        {
            return "Usage недоступен";
        }

        EnsurePlayerUiFields(player);
        return FormatPlayerCompact(player)
            + "\n" + SafeText(player.PlayerRole)
            + " | " + SafeText(player.UsageCategory)
            + " | TOI " + IceTimeConfig.FormatSeconds(player.EstimatedTimeOnIceSeconds);
    }

    public static string FormatPlayerWithHealth(PlayerData player)
    {
        if (player == null)
        {
            return "Здоровье недоступно";
        }

        EnsurePlayerUiFields(player);
        string injury = player.IsInjured
            ? " | INJ " + player.InjuryType + " " + player.InjuryDaysRemaining + "d"
            : "";
        return FormatPlayerCompact(player)
            + "\nCOND " + player.Condition
            + " | FAT " + player.Fatigue
            + injury;
    }

    public static string FormatFreeAgent(PlayerData player)
    {
        if (player == null)
        {
            return "Свободный агент недоступен";
        }

        return FormatPlayerMainLine(player)
            + "\nExpected " + MobileUiConfig.FormatMoney(player.FreeAgencyExpectedSalary)
            + " x " + player.FreeAgencyExpectedYears
            + " | Min " + MobileUiConfig.FormatMoney(player.FreeAgencyMinimumSalary)
            + " | INT " + player.FreeAgencyInterestInUserTeam
            + " | Last " + (string.IsNullOrEmpty(player.LastFreeAgencyOfferStatus) ? "None" : player.LastFreeAgencyOfferStatus);
    }

    public static string FormatProspect(ProspectData prospect)
    {
        if (prospect == null)
        {
            return "Проспект недоступен";
        }

        int rank = ScoutingService.GetProspectRank(prospect, prospect.ProjectedPick);
        ScoutingService.EnsureProspectScouting(prospect, rank);
        string pinned = prospect.IsUserPinned ? " | PIN" : "";
        return "Rank #" + rank
            + " | " + prospect.FirstName + " " + prospect.LastName
            + " | " + prospect.Position
            + " | OVR " + FormatProspectOverall(prospect)
            + " | POT " + FormatProspectPotential(prospect)
            + "\nACC " + MobileUiConfig.FormatPercent(prospect.ScoutingAccuracy)
            + " | Risk " + SafeText(prospect.RiskHint)
            + " | " + SafeText(prospect.ProspectArchetype)
            + " | " + SafeText(prospect.ProjectedRole)
            + pinned;
    }

    public static string FormatPlayerName(PlayerData player)
    {
        return GetPlayerName(player);
    }

    private static void EnsurePlayerUiFields(PlayerData player)
    {
        if (player == null)
        {
            return;
        }

        PlayerFatigueService.EnsureFatigueFields(player);
        InjuryService.EnsureInjuryFields(player);
        PlayerRoleService.EnsureRole(player);
        MoraleService.InitializePlayerMorale(player);
        LeadershipService.EnsurePlayerLeadershipProfile(player);
    }

    private static string FormatProspectOverall(ProspectData prospect)
    {
        return prospect.IsFullyScouted
            ? prospect.Overall.ToString()
            : prospect.EstimatedOverallMin + "-" + prospect.EstimatedOverallMax;
    }

    private static string FormatProspectPotential(ProspectData prospect)
    {
        return prospect.IsFullyScouted
            ? prospect.Potential.ToString()
            : prospect.EstimatedPotentialMin + "-" + prospect.EstimatedPotentialMax;
    }

    private static string GetPlayerName(PlayerData player)
    {
        return player == null
            ? ""
            : (SafeText(player.FirstName) + " " + SafeText(player.LastName)).Trim();
    }

    private static string FormatJerseyNumber(PlayerData player)
    {
        return player != null && player.JerseyNumber > 0 ? "#" + player.JerseyNumber + " " : "";
    }

    private static string SafeText(string value)
    {
        return string.IsNullOrEmpty(value) ? "n/a" : value;
    }
}
