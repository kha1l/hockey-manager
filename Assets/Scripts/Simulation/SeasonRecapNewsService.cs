public static class SeasonRecapNewsService
{
    public static void GenerateNewsForSeasonRecap(GameState state, LeagueSeasonHistoryData history)
    {
        if (state == null || history == null)
        {
            return;
        }

        CreateSeasonRecapNews(state, history);
        CreateAwardNews(state, history.Awards);
        CreateRecordNews(state, state.LeagueRecords, history.SeasonStartYear);

        TeamData userTeam = FindTeam(state, state.SelectedTeamId);
        OwnerProfileData profile = userTeam == null ? null : OwnerGoalService.GetOwnerProfile(state, userTeam);
        if (profile != null && profile.LastSeasonEvaluation != null)
        {
            CreateOwnerEvaluationNews(state, profile.LastSeasonEvaluation);
        }
    }

    public static NewsItemData CreateSeasonRecapNews(GameState state, LeagueSeasonHistoryData history)
    {
        if (state == null || history == null)
        {
            return null;
        }

        string title = string.IsNullOrEmpty(history.ChampionTeamName)
            ? "Season " + FormatSeason(history.SeasonStartYear, history.SeasonEndYear) + " recap"
            : history.ChampionTeamName + " wins the championship";

        return NewsFeedService.AddNews(
            state,
            NewsConfig.CategorySeasonRecap,
            title,
            BuildSeasonRecapBody(history),
            95,
            history.ChampionTeamId,
            history.ChampionTeamName,
            "",
            "",
            history.HistoryId);
    }

    public static void CreateAwardNews(GameState state, SeasonAwardsData awards)
    {
        if (state == null || awards == null || awards.Awards == null)
        {
            return;
        }

        foreach (AwardWinnerData award in awards.Awards)
        {
            if (award == null)
            {
                continue;
            }

            NewsFeedService.AddNews(
                state,
                NewsConfig.CategoryAward,
                SafeText(award.PlayerName, "Unknown player") + " wins " + SafeText(award.AwardName, "award"),
                BuildAwardBody(award),
                GetAwardImportance(award.AwardType),
                award.TeamId,
                award.TeamName,
                award.PlayerId,
                award.PlayerName,
                award.AwardId);
        }
    }

    public static void CreateRecordNews(GameState state, LeagueRecordsData records, int seasonStartYear)
    {
        if (state == null || records == null || records.Records == null)
        {
            return;
        }

        foreach (LeagueRecordData record in records.Records)
        {
            if (record == null || record.SeasonStartYear != seasonStartYear)
            {
                continue;
            }

            NewsFeedService.AddNews(
                state,
                NewsConfig.CategoryRecord,
                SafeText(record.PlayerName, "A player") + " sets " + SafeText(record.RecordName, "a record"),
                BuildRecordBody(record),
                80,
                record.TeamId,
                record.TeamName,
                record.PlayerId,
                record.PlayerName,
                record.RecordId);
        }
    }

    public static void CreateOwnerEvaluationNews(GameState state, OwnerSeasonEvaluationData evaluation)
    {
        if (state == null || evaluation == null)
        {
            return;
        }

        string title = "Owner evaluates the season";
        int importance = 60;
        if (evaluation.TrustDelta >= 10)
        {
            title = "Owner pleased with season result";
            importance = 80;
        }
        else if (evaluation.TrustDelta <= -10)
        {
            title = "Owner disappointed after season";
            importance = 80;
        }

        NewsFeedService.AddNews(
            state,
            NewsConfig.CategoryOwner,
            title,
            BuildOwnerEvaluationBody(evaluation),
            importance,
            state.SelectedTeamId,
            evaluation.TeamName,
            "",
            "",
            "OwnerEvaluation_" + evaluation.SeasonStartYear + "_" + evaluation.TeamId);
    }

    public static string BuildSeasonRecapBody(LeagueSeasonHistoryData history)
    {
        if (history == null)
        {
            return "Сезон завершён.";
        }

        return "Сезон " + FormatSeason(history.SeasonStartYear, history.SeasonEndYear)
            + " завершён: " + SafeText(history.ChampionTeamName, "чемпион неизвестен")
            + " стала чемпионом"
            + (string.IsNullOrEmpty(history.FinalistTeamName) ? "." : ", обыграв " + history.FinalistTeamName + ".")
            + " " + SafeText(history.MvpPlayerName, "MVP не выбран")
            + " стал MVP, а " + SafeText(history.TopScorerPlayerName, "лучший бомбардир не определён")
            + " возглавил лигу с " + history.TopScorerPoints + " очками."
            + " Команда пользователя завершила сезон: " + SafeText(history.UserTeamResult, "нет данных") + ".";
    }

    public static string BuildAwardBody(AwardWinnerData award)
    {
        if (award == null)
        {
            return "Награда вручена.";
        }

        return SafeText(award.PlayerName, "Игрок")
            + " получил " + SafeText(award.AwardName, "награду")
            + " после сезона " + FormatSeason(award.SeasonStartYear, award.SeasonEndYear)
            + ". Причина: " + SafeText(award.Reason, "сильное выступление") + ".";
    }

    public static string BuildRecordBody(LeagueRecordData record)
    {
        if (record == null)
        {
            return "Новый рекорд зафиксирован.";
        }

        return SafeText(record.PlayerName, "Игрок")
            + " установил рекорд: " + SafeText(record.RecordName, "Record")
            + " (" + SafeText(record.ValueLabel, record.Value.ToString()) + ").";
    }

    public static string BuildOwnerEvaluationBody(OwnerSeasonEvaluationData evaluation)
    {
        if (evaluation == null)
        {
            return "Владелец оценил сезон.";
        }

        return "Владелец оценил сезон: trust "
            + evaluation.TrustBefore + " -> " + evaluation.TrustAfter
            + " (" + FormatSigned(evaluation.TrustDelta) + "), goals "
            + evaluation.GoalsCompleted + "/" + (evaluation.GoalsCompleted + evaluation.GoalsFailed)
            + ", job security: " + SafeText(evaluation.JobSecurity, "нет данных") + ".";
    }

    private static int GetAwardImportance(string awardType)
    {
        if (awardType == AwardsConfig.LeagueMvp)
        {
            return 85;
        }

        if (awardType == AwardsConfig.PlayoffMvp)
        {
            return 80;
        }

        if (awardType == AwardsConfig.TopScorer || awardType == AwardsConfig.BestGoalie)
        {
            return 75;
        }

        return 65;
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

    private static string FormatSeason(int startYear, int endYear)
    {
        return startYear + "-" + (endYear % 100).ToString("D2");
    }

    private static string FormatSigned(int value)
    {
        return value >= 0 ? "+" + value : value.ToString();
    }

    private static string SafeText(string value, string fallback)
    {
        return string.IsNullOrEmpty(value) ? fallback : value;
    }
}
