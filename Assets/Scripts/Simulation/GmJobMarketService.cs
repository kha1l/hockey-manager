using System;
using System.Collections.Generic;

public static class GmJobMarketService
{
    public static List<GmJobOfferData> GenerateJobOffers(GameState state)
    {
        GmCareerService.EnsureGmCareer(state);
        if (state == null)
        {
            return new List<GmJobOfferData>();
        }

        ClearExpiredOffers(state);
        List<TeamData> candidates = FindTeamsWithPotentialVacancies(state);
        candidates.Sort((left, right) => CalculateTeamVacancyScore(state, right).CompareTo(CalculateTeamVacancyScore(state, left)));

        state.ActiveGmJobOffers = new List<GmJobOfferData>();
        foreach (TeamData team in candidates)
        {
            if (team == null || state.ActiveGmJobOffers.Count >= GmJobSecurityConfig.MaxJobOffers)
            {
                break;
            }

            GmJobOfferData offer = CreateJobOffer(state, team, BuildOfferReason(state, team));
            state.ActiveGmJobOffers.Add(offer);
        }

        foreach (GmJobOfferData offer in state.ActiveGmJobOffers)
        {
            GmCareerService.AddCareerEvent(
                state,
                GmCareerService.CreateCareerEvent(
                    state,
                    "JobOffer",
                    offer.TeamId,
                    offer.TeamName,
                    "Job offer from " + offer.TeamName,
                    offer.OfferReason,
                    0,
                    offer.OwnerTrustStartingValue,
                    0,
                    offer.JobSecurityStartingValue));
        }

        return new List<GmJobOfferData>(state.ActiveGmJobOffers);
    }

    public static GmJobOfferData CreateJobOffer(GameState state, TeamData team, string reason)
    {
        string direction = TeamDirectionService.DetermineTeamDirection(state, team);
        int overall = TeamRatingCalculator.CalculateOverall(team);
        int points = GetRegularSeasonPoints(state, team);
        bool madePlayoffs = OwnerGoalProgressService.DidTeamMakePlayoffs(state, team);
        int startingTrust = CalculateStartingTrust(direction, madePlayoffs, points);
        string teamName = GetTeamName(team);
        DateTime createdAt = DateTime.UtcNow;

        OwnerProfileData owner = OwnerGoalService.GetOwnerProfile(state, team);
        string expectations = owner == null || string.IsNullOrEmpty(owner.ExpectationsSummary)
            ? "Build toward ownership expectations"
            : owner.ExpectationsSummary;

        return new GmJobOfferData
        {
            OfferId = Guid.NewGuid().ToString("N"),
            TeamId = team == null ? "" : team.Id,
            TeamName = teamName,
            TeamDirection = direction,
            TeamOverall = overall,
            LastSeasonPoints = points,
            MadePlayoffsLastSeason = madePlayoffs,
            OwnerTrustStartingValue = startingTrust,
            JobSecurityStartingValue = GmJobSecurityConfig.ClampSecurity(startingTrust),
            OfferReason = string.IsNullOrEmpty(reason) ? BuildOfferReason(state, team) : reason,
            ChallengeSummary = BuildChallengeSummary(state, team, direction, overall),
            ExpectationsSummary = expectations,
            IsAccepted = false,
            IsDeclined = false,
            CreatedAtUtc = createdAt.ToString("o"),
            ExpiresAtUtc = createdAt.AddDays(GmJobSecurityConfig.JobOfferExpiryDays).ToString("o")
        };
    }

    public static List<TeamData> FindTeamsWithPotentialVacancies(GameState state)
    {
        List<TeamData> teams = new List<TeamData>();
        if (state == null || state.Teams == null)
        {
            return teams;
        }

        foreach (TeamData team in state.Teams)
        {
            if (IsTeamAvailableForGm(state, team) && CalculateTeamVacancyScore(state, team) > 0)
            {
                teams.Add(team);
            }
        }

        return teams;
    }

    public static int CalculateTeamVacancyScore(GameState state, TeamData team)
    {
        if (state == null || team == null || team.Id == state.SelectedTeamId)
        {
            return 0;
        }

        int score = 20;
        OwnerProfileData owner = OwnerGoalService.GetOwnerProfile(state, team);
        if (owner != null)
        {
            if (owner.GmTrust < 40)
            {
                score += 35;
            }
            else if (owner.GmTrust < 55)
            {
                score += 15;
            }
        }

        int points = GetRegularSeasonPoints(state, team);
        if (points > 0)
        {
            if (points < 70)
            {
                score += 30;
            }
            else if (points < 85)
            {
                score += 15;
            }
        }

        string direction = TeamDirectionService.DetermineTeamDirection(state, team);
        bool madePlayoffs = OwnerGoalProgressService.DidTeamMakePlayoffs(state, team);
        if (!madePlayoffs && (direction == TradeAiConfig.DirectionContender || direction == TradeAiConfig.DirectionPlayoffTeam))
        {
            score += 30;
        }
        else if (direction == TradeAiConfig.DirectionRebuild || direction == TradeAiConfig.DirectionRetool)
        {
            score += 10;
        }

        GmCareerData career = state.GmCareer;
        if (career != null && career.IsFired && team.Id == career.FiredFromTeamId)
        {
            score = 0;
        }

        return Clamp(score, 0, 100);
    }

    public static bool IsTeamAvailableForGm(GameState state, TeamData team)
    {
        if (state == null || team == null || string.IsNullOrEmpty(team.Id))
        {
            return false;
        }

        if (team.Id == state.SelectedTeamId)
        {
            return false;
        }

        GmCareerData career = state.GmCareer;
        return career == null || team.Id != career.FiredFromTeamId;
    }

    public static void ClearExpiredOffers(GameState state)
    {
        if (state == null)
        {
            return;
        }

        GmCareerService.EnsureCareerLists(state);
        DateTime now = DateTime.UtcNow;
        for (int i = state.ActiveGmJobOffers.Count - 1; i >= 0; i--)
        {
            GmJobOfferData offer = state.ActiveGmJobOffers[i];
            if (offer == null || offer.IsAccepted || offer.IsDeclined || IsExpired(offer, now))
            {
                state.ActiveGmJobOffers.RemoveAt(i);
            }
        }
    }

    public static bool AcceptJobOffer(GameState state, string offerId, out string message)
    {
        GmCareerService.EnsureGmCareer(state);
        GmJobOfferData offer = FindOffer(state, offerId);
        if (state == null || offer == null)
        {
            message = "Предложение работы не найдено";
            return false;
        }

        TeamData newTeam = FindTeam(state, offer.TeamId);
        if (newTeam == null)
        {
            message = "Команда предложения не найдена";
            return false;
        }

        state.SelectedTeamId = offer.TeamId;
        offer.IsAccepted = true;
        state.ActiveGmJobOffers = new List<GmJobOfferData>();

        GmCareerData career = state.GmCareer;
        career.CurrentTeamId = offer.TeamId;
        career.CurrentTeamName = offer.TeamName;
        career.IsFired = false;
        career.IsUnemployed = false;
        career.CareerStatus = "Hired";
        career.CurrentJobSecurity = GmJobSecurityConfig.ClampSecurity(offer.JobSecurityStartingValue);
        career.CurrentOwnerTrust = OwnerGoalConfig.ClampTrust(offer.OwnerTrustStartingValue);
        career.UpdatedAtUtc = DateTime.UtcNow.ToString("o");
        GmCareerService.AddManagedTeam(career, offer.TeamId, offer.TeamName);

        OwnerProfileData owner = OwnerGoalService.EnsureOwnerProfile(state, newTeam);
        owner.GmTrust = career.CurrentOwnerTrust;
        owner.JobSecurity = OwnerGoalConfig.GetJobSecurityLabel(owner.GmTrust);
        owner.CurrentGoals = OwnerGoalGenerator.GenerateSeasonGoals(state, newTeam);
        owner.ExpectationsSummary = OwnerGoalService.BuildExpectationsSummary(owner);
        owner.UpdatedAtUtc = DateTime.UtcNow.ToString("o");

        TeamRosterService.EnsureRosterStatusesForTeam(newTeam);
        LineupService.EnsureLineup(newTeam);
        MoraleService.EnsureMoraleForTeam(state, newTeam);
        ChemistryService.EnsureChemistryForTeam(newTeam);
        LeadershipService.EnsureLeadershipForTeam(newTeam);
        CoachingStaffService.EnsureStaffForTeam(newTeam);

        GmCareerEventData careerEvent = GmCareerService.CreateCareerEvent(
            state,
            "Hired",
            offer.TeamId,
            offer.TeamName,
            "GM hired by " + offer.TeamName,
            "Вы приняли работу в " + offer.TeamName,
            0,
            career.CurrentOwnerTrust,
            0,
            career.CurrentJobSecurity);
        GmCareerService.AddCareerEvent(state, careerEvent);
        EventNewsService.CreateGmHiringNews(state, careerEvent);

        message = "Вы приняли работу в " + offer.TeamName;
        return true;
    }

    public static bool DeclineJobOffer(GameState state, string offerId, out string message)
    {
        GmCareerService.EnsureGmCareer(state);
        GmJobOfferData offer = FindOffer(state, offerId);
        if (state == null || offer == null)
        {
            message = "Предложение работы не найдено";
            return false;
        }

        offer.IsDeclined = true;
        state.ActiveGmJobOffers.Remove(offer);
        GmCareerService.AddCareerEvent(
            state,
            GmCareerService.CreateCareerEvent(
                state,
                "JobOffer",
                offer.TeamId,
                offer.TeamName,
                "Job offer declined",
                "Вы отклонили предложение от " + offer.TeamName,
                0,
                0,
                0,
                0));
        message = "Предложение от " + offer.TeamName + " отклонено";
        return true;
    }

    public static GmJobOfferData FindOffer(GameState state, string offerId)
    {
        if (state == null || state.ActiveGmJobOffers == null || string.IsNullOrEmpty(offerId))
        {
            return null;
        }

        foreach (GmJobOfferData offer in state.ActiveGmJobOffers)
        {
            if (offer != null && offer.OfferId == offerId)
            {
                return offer;
            }
        }

        return null;
    }

    private static bool IsExpired(GmJobOfferData offer, DateTime now)
    {
        if (offer == null || string.IsNullOrEmpty(offer.ExpiresAtUtc))
        {
            return false;
        }

        return DateTime.TryParse(offer.ExpiresAtUtc, out DateTime expiresAt) && expiresAt <= now;
    }

    private static string BuildOfferReason(GameState state, TeamData team)
    {
        string direction = TeamDirectionService.DetermineTeamDirection(state, team);
        bool madePlayoffs = OwnerGoalProgressService.DidTeamMakePlayoffs(state, team);
        if (!madePlayoffs && (direction == TradeAiConfig.DirectionContender || direction == TradeAiConfig.DirectionPlayoffTeam))
        {
            return "Team missed expectations";
        }

        if (direction == TradeAiConfig.DirectionRebuild)
        {
            return "Rebuild opportunity";
        }

        return "Roster needs new direction";
    }

    private static string BuildChallengeSummary(GameState state, TeamData team, string direction, int overall)
    {
        TeamFinanceData finance = SalaryCapService.CalculateTeamFinance(team);
        return direction + " roster"
            + " | OVR " + overall
            + " | Cap space " + MobileUiConfig.FormatMoney(finance == null ? 0 : finance.CapSpace)
            + " | Last points " + GetRegularSeasonPoints(state, team);
    }

    private static int CalculateStartingTrust(string direction, bool madePlayoffs, int points)
    {
        int value = 65;
        if (direction == TradeAiConfig.DirectionRebuild)
        {
            value = 60;
        }
        else if (direction == TradeAiConfig.DirectionContender)
        {
            value = 52;
        }

        if (madePlayoffs)
        {
            value += 5;
        }

        if (points > 0 && points < 70)
        {
            value -= 5;
        }

        return GmJobSecurityConfig.ClampSecurity(value);
    }

    private static int GetRegularSeasonPoints(GameState state, TeamData team)
    {
        if (state == null || state.Season == null || state.Season.Standings == null || team == null)
        {
            return 0;
        }

        foreach (TeamStandingData standing in state.Season.Standings)
        {
            if (standing != null && standing.TeamId == team.Id)
            {
                return standing.Points;
            }
        }

        return 0;
    }

    private static TeamData FindTeam(GameState state, string teamId)
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

    private static string GetTeamName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }

    private static int Clamp(int value, int min, int max)
    {
        if (value < min)
        {
            return min;
        }

        return value > max ? max : value;
    }
}
