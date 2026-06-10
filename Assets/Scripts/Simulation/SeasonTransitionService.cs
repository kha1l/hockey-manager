using System;
using System.Collections.Generic;

public static class SeasonTransitionService
{
    public static bool CanStartNextSeason(GameState state, out string message)
    {
        if (state == null)
        {
            message = "Состояние игры не найдено";
            return false;
        }

        if (!LeaguePhaseService.CanStartNextSeason(state))
        {
            message = "Следующий сезон станет доступен после завершения плей-офф и драфта";
            return false;
        }

        message = "Можно начать следующий сезон";
        return true;
    }

    public static bool StartNextSeason(GameState state, out string message)
    {
        if (!CanStartNextSeason(state, out message))
        {
            return false;
        }

        state.EnsureCareerProgress();
        SeasonHistoryService.ArchiveCurrentSeasonIfNeeded(state);
        OffseasonContractService.AdvanceAgesAndContracts(state);
        PlayerDevelopmentService.ApplyYearlyDevelopment(state);
        OffseasonContractService.MoveExpiredUfaPlayersToFreeAgency(state);
        OffseasonContractService.NormalizeRfaPlayers(state);

        state.CurrentSeasonStartYear++;
        state.CurrentSeasonEndYear++;
        state.CareerSeasonNumber++;

        CreateNextLeagueCalendar(state);
        ResetSeasonSystemsForNextYear(state);
        CreateNextSeason(state);

        ContractGenerator.EnsureContractsForTeams(state.Teams);
        FreeAgentService.EnsureFreeAgentData(state);
        LineupService.EnsureLineupsForTeams(state.Teams);
        SpecialTeamsService.EnsureSpecialTeamsForTeams(state.Teams);
        TacticsService.EnsureTacticsForTeams(state.Teams);
        PlayerFatigueService.ResetFatigueForNewSeason(state.Teams);
        InjuryService.ResetInjuriesForNewSeason(state);
        ResetPlayerUsageForNewSeason(state.Teams);
        PlayerRoleService.EnsureRolesForTeams(state.Teams);
        IceTimeService.EnsureUsageForTeams(state.Teams);

        message = "Следующий сезон начат";
        return true;
    }

    private static void ResetSeasonSystemsForNextYear(GameState state)
    {
        state.MatchHistory = new List<MatchResultData>();
        state.TotalGamesSimulated = 0;
        state.LastMatchResult = null;
        state.Draft = null;
        state.DraftHistory = new DraftHistoryData();
        state.DraftPickOwnership = new List<DraftPickOwnershipData>();
    }

    private static void CreateNextLeagueCalendar(GameState state)
    {
        LeagueCalendarData previousCalendar = state.LeagueCalendar ?? LeagueCalendarConfig.CreateDefaultCalendar();
        LeagueCalendarData defaultCalendar = LeagueCalendarConfig.CreateDefaultCalendar();

        state.LeagueCalendar = new LeagueCalendarData
        {
            CalendarStatus = "Provisional",
            SeasonStartYear = state.CurrentSeasonStartYear,
            SeasonEndYear = state.CurrentSeasonEndYear,
            PreseasonStartDate = ShiftDateOrDefault(previousCalendar.PreseasonStartDate, defaultCalendar.PreseasonStartDate, state.CurrentSeasonStartYear),
            PreseasonEndDate = ShiftDateOrDefault(previousCalendar.PreseasonEndDate, defaultCalendar.PreseasonEndDate, state.CurrentSeasonStartYear),
            RegularSeasonStartDate = ShiftDateOrDefault(previousCalendar.RegularSeasonStartDate, defaultCalendar.RegularSeasonStartDate, state.CurrentSeasonStartYear),
            RegularSeasonEndDate = ShiftDateOrDefault(previousCalendar.RegularSeasonEndDate, defaultCalendar.RegularSeasonEndDate, state.CurrentSeasonStartYear),
            TradeDeadlineDate = ShiftDateOrDefault(previousCalendar.TradeDeadlineDate, defaultCalendar.TradeDeadlineDate, state.CurrentSeasonStartYear),
            PlayoffsStartDate = ShiftDateOrDefault(previousCalendar.PlayoffsStartDate, defaultCalendar.PlayoffsStartDate, state.CurrentSeasonStartYear),
            StanleyCupFinalExpectedEndDate = ShiftDateOrDefault(previousCalendar.StanleyCupFinalExpectedEndDate, defaultCalendar.StanleyCupFinalExpectedEndDate, state.CurrentSeasonStartYear),
            DraftStartDate = ShiftDateOrDefault(previousCalendar.DraftStartDate, defaultCalendar.DraftStartDate, state.CurrentSeasonStartYear),
            DraftEndDate = ShiftDateOrDefault(previousCalendar.DraftEndDate, defaultCalendar.DraftEndDate, state.CurrentSeasonStartYear),
            FreeAgencyStartDate = ShiftDateOrDefault(previousCalendar.FreeAgencyStartDate, defaultCalendar.FreeAgencyStartDate, state.CurrentSeasonStartYear)
        };
    }

    private static void CreateNextSeason(GameState state)
    {
        if (state.Teams == null)
        {
            state.Teams = TeamSeedData.CreateTeams();
        }

        state.Season = SeasonGenerator.CreateSimpleSeason(
            state.SelectedTeamId,
            state.Teams,
            state.CurrentSeasonStartYear);
        state.Season.CurrentDay = 1;
        state.Season.IsSeasonFinished = false;
        state.Season.Playoffs = null;
        state.Season.PlayerStats = new List<PlayerSeasonStatsData>();
        state.Season.EnsureCollections();

        DraftPickOwnershipService.EnsureDraftPickOwnership(state);
    }

    private static void ResetPlayerUsageForNewSeason(List<TeamData> teams)
    {
        if (teams == null)
        {
            return;
        }

        foreach (TeamData team in teams)
        {
            if (team == null)
            {
                continue;
            }

            team.EnsurePlayers();
            foreach (PlayerData player in team.Players)
            {
                if (player == null)
                {
                    continue;
                }

                player.LastGameTimeOnIceSeconds = 0;
                player.AverageTimeOnIceSeconds = 0;
                player.TotalTimeOnIceSeconds = 0;
                player.GamesWithTimeOnIce = 0;
            }
        }
    }

    private static string ShiftDateOrDefault(string previousDate, string fallbackDate, int targetSeasonStartYear)
    {
        if (DateTime.TryParse(previousDate, out DateTime parsedPreviousDate))
        {
            return parsedPreviousDate.AddYears(1).ToString("yyyy-MM-dd");
        }

        if (DateTime.TryParse(fallbackDate, out DateTime parsedFallbackDate))
        {
            int yearOffset = targetSeasonStartYear - SalaryCapConfig.RulesSeasonStartYear;
            return parsedFallbackDate.AddYears(yearOffset).ToString("yyyy-MM-dd");
        }

        return fallbackDate;
    }
}
