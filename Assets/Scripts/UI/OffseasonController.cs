using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class OffseasonController : MonoBehaviour
{
    [SerializeField] private Text _statusText;
    [SerializeField] private Text _summaryText;
    [SerializeField] private Transform _historyContainer;
    [SerializeField] private SeasonHistoryRowView _historyRowPrefab;

    public void Configure(
        Text statusText,
        Text summaryText,
        Transform historyContainer,
        SeasonHistoryRowView historyRowPrefab)
    {
        _statusText = statusText;
        _summaryText = summaryText;
        _historyContainer = historyContainer;
        _historyRowPrefab = historyRowPrefab;
    }

    public void ShowOffseason(GameState state)
    {
        if (_statusText == null || _summaryText == null || _historyContainer == null || _historyRowPrefab == null)
        {
            Debug.LogError("OffseasonController: UI references are not configured.");
            return;
        }

        SeasonHistoryService.EnsureSeasonHistory(state);
        RenderStatus(state);
        RenderSummary(state);
        RenderHistory(state);
    }

    private void RenderStatus(GameState state)
    {
        string phase = LeaguePhaseService.GetCurrentPhase(state);
        string currentSeason = state == null
            ? "2026-27"
            : FormatSeason(state.CurrentSeasonStartYear, state.CurrentSeasonEndYear);
        string nextSeason = state == null
            ? "2027-28"
            : FormatSeason(state.CurrentSeasonStartYear + 1, state.CurrentSeasonEndYear + 1);
        bool canStartNextSeason = SeasonTransitionService.CanStartNextSeason(state, out string message);
        string status = canStartNextSeason ? "Следующий сезон доступен" : "Завершите плей-офф и драфт";
        string champion = GetChampionText(state);
        LeagueSeasonHistoryData lastLeagueHistory = state == null ? null : state.LastLeagueSeasonHistory;

        _statusText.text = "Фаза: " + phase
            + "\nТекущий сезон: " + currentSeason
            + "\nСледующий сезон: " + nextSeason
            + "\nСтатус: " + status
            + "\n" + message
            + "\nЧемпион прошлого сезона: " + champion
            + "\nПоследний recap: " + FormatLeagueRecapStatus(lastLeagueHistory)
            + "\nПеред переходом сезона владелец оценит текущий сезон.";
    }

    private void RenderSummary(GameState state)
    {
        TeamData team = GetUserTeam(state);
        if (team == null)
        {
            _summaryText.text = "Команда не найдена";
            return;
        }

        team.EnsurePlayers();
        ContractExtensionService.EnsureExtensionDataForTeam(state, team);
        TeamFinanceData finance = SalaryCapService.CalculateTeamFinance(team);
        OwnerProfileData ownerProfile = OwnerGoalService.GetOwnerProfile(state, team);
        OwnerSeasonEvaluationData lastEvaluation = ownerProfile == null ? null : ownerProfile.LastSeasonEvaluation;
        CountContractStatuses(team.Players, out int expiringCount, out int rfaCount, out int ufaCount);
        ContractExtensionSummaryData extensionSummary = ContractExtensionService.BuildSummary(state, team);
        LeagueSeasonHistoryData lastLeagueHistory = state == null ? null : state.LastLeagueSeasonHistory;

        _summaryText.text = "Expiring: " + expiringCount
            + " | RFA: " + rfaCount
            + " | UFA: " + ufaCount
            + "\nВнимание: игроки с истекающими контрактами без продления могут стать UFA/RFA"
            + "\nEligible extensions: " + (extensionSummary == null ? 0 : extensionSummary.EligiblePlayers)
            + " | Low interest: " + (extensionSummary == null ? 0 : extensionSummary.LowInterestCount)
            + "\nОткройте панель Продления перед переходом сезона"
            + "\nRoster: " + finance.PlayerCount + " / " + SalaryCapConfig.MaxRosterSize
            + "\nPayroll: " + FormatMoney(finance.Payroll)
            + " | Cap space: " + FormatMoney(finance.CapSpace)
            + "\nOwner: trust " + (ownerProfile == null ? 0 : ownerProfile.GmTrust)
            + " | satisfaction " + (ownerProfile == null ? 0 : ownerProfile.OwnerSatisfaction)
            + " | job security " + (ownerProfile == null ? "нет данных" : ownerProfile.JobSecurity)
            + "\nLast owner evaluation: " + FormatOwnerEvaluation(lastEvaluation)
            + "\nNext season goals: " + FormatCurrentOwnerGoals(ownerProfile)
            + "\nSeason recap: " + FormatLeagueRecapSummary(state, lastLeagueHistory)
            + "\nRecap news: " + FormatLatestRecapNews(state);
    }

    private void RenderHistory(GameState state)
    {
        ClearRows();
        _historyRowPrefab.gameObject.SetActive(false);

        if (state == null || state.SeasonHistory == null || state.SeasonHistory.Count == 0)
        {
            CreateInfoRow("История сезонов пока пуста");
            return;
        }

        int firstIndex = Mathf.Max(0, state.SeasonHistory.Count - 10);
        for (int i = state.SeasonHistory.Count - 1; i >= firstIndex; i--)
        {
            SeasonHistoryData history = state.SeasonHistory[i];
            if (history == null)
            {
                continue;
            }

            history.EnsureCollections();
            SeasonHistoryRowView row = Instantiate(_historyRowPrefab, _historyContainer);
            row.name = "season-history-" + history.SeasonStartYear;
            row.gameObject.SetActive(true);
            row.Initialize(history);
        }
    }

    private void ClearRows()
    {
        Transform prefabTransform = _historyRowPrefab.transform;
        for (int i = _historyContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = _historyContainer.GetChild(i);
            if (child == prefabTransform)
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }

    private void CreateInfoRow(string value)
    {
        GameObject rowObject = new GameObject("InfoRow");
        rowObject.transform.SetParent(_historyContainer, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 44f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 44f;
        layoutElement.minHeight = 44f;

        Text text = rowObject.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 16;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
    }

    private static TeamData GetUserTeam(GameState state)
    {
        if (state == null || state.Teams == null || string.IsNullOrEmpty(state.SelectedTeamId))
        {
            return null;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team != null && team.Id == state.SelectedTeamId)
            {
                return team;
            }
        }

        return null;
    }

    private static string GetChampionText(GameState state)
    {
        if (state != null
            && state.Season != null
            && state.Season.Playoffs != null
            && !string.IsNullOrEmpty(state.Season.Playoffs.ChampionTeamName))
        {
            return state.Season.Playoffs.ChampionTeamName;
        }

        SeasonHistoryData lastHistory = GetLastHistory(state);
        return lastHistory == null || string.IsNullOrEmpty(lastHistory.ChampionTeamName)
            ? "нет данных"
            : lastHistory.ChampionTeamName;
    }

    private static SeasonHistoryData GetLastHistory(GameState state)
    {
        if (state == null || state.SeasonHistory == null || state.SeasonHistory.Count == 0)
        {
            return null;
        }

        for (int i = state.SeasonHistory.Count - 1; i >= 0; i--)
        {
            if (state.SeasonHistory[i] != null)
            {
                return state.SeasonHistory[i];
            }
        }

        return null;
    }

    private static string FormatLatestRecapNews(GameState state)
    {
        List<NewsItemData> news = NewsFeedService.GetNewsByCategory(state, NewsConfig.CategorySeasonRecap, 1);
        if (news == null || news.Count == 0 || news[0] == null)
        {
            return "нет";
        }

        return string.IsNullOrEmpty(news[0].Title) ? "нет данных" : news[0].Title;
    }

    private static void CountContractStatuses(List<PlayerData> players, out int expiringCount, out int rfaCount, out int ufaCount)
    {
        expiringCount = 0;
        rfaCount = 0;
        ufaCount = 0;

        if (players == null)
        {
            return;
        }

        foreach (PlayerData player in players)
        {
            if (player == null)
            {
                continue;
            }

            if (player.ContractStatus == "Expiring")
            {
                expiringCount++;
            }
            else if (player.ContractStatus == "RFA")
            {
                rfaCount++;
            }
            else if (player.ContractStatus == "UFA")
            {
                ufaCount++;
            }
        }
    }

    private static string FormatOwnerEvaluation(OwnerSeasonEvaluationData evaluation)
    {
        if (evaluation == null)
        {
            return "ещё не проводилась";
        }

        string delta = evaluation.TrustDelta >= 0 ? "+" + evaluation.TrustDelta : evaluation.TrustDelta.ToString();
        return "trust " + evaluation.TrustBefore + " -> " + evaluation.TrustAfter
            + " (" + delta + ")"
            + " | goals " + evaluation.GoalsCompleted + "/" + (evaluation.GoalsCompleted + evaluation.GoalsFailed)
            + " | " + evaluation.JobSecurity;
    }

    private static string FormatCurrentOwnerGoals(OwnerProfileData profile)
    {
        if (profile == null || profile.CurrentGoals == null || profile.CurrentGoals.Count == 0)
        {
            return "нет данных";
        }

        foreach (OwnerGoalData goal in profile.CurrentGoals)
        {
            if (goal != null && goal.GoalType == OwnerGoalConfig.GoalTypePrimary)
            {
                return goal.Title + " (" + goal.ProgressPercent + "%)";
            }
        }

        return profile.CurrentGoals[0] == null ? "нет данных" : profile.CurrentGoals[0].Title;
    }

    private static string FormatLeagueRecapStatus(LeagueSeasonHistoryData history)
    {
        if (history == null)
        {
            return "ещё не создан";
        }

        return FormatSeason(history.SeasonStartYear, history.SeasonEndYear)
            + " | champion " + SafeText(history.ChampionTeamName)
            + " | finalist " + SafeText(history.FinalistTeamName);
    }

    private static string FormatLeagueRecapSummary(GameState state, LeagueSeasonHistoryData history)
    {
        if (history == null)
        {
            return "итоги сезона появятся после завершения плей-офф и драфта";
        }

        AwardWinnerData mvp = FindAward(state == null ? null : state.LastSeasonAwards, AwardsConfig.LeagueMvp);
        string mvpName = mvp == null ? history.MvpPlayerName : mvp.PlayerName;
        int awardsCount = state == null || state.LastSeasonAwards == null || state.LastSeasonAwards.Awards == null
            ? 0
            : state.LastSeasonAwards.Awards.Count;
        int recordsCount = state == null || state.LeagueRecords == null || state.LeagueRecords.Records == null
            ? 0
            : state.LeagueRecords.Records.Count;

        return "Champion " + SafeText(history.ChampionTeamName)
            + " | Finalist " + SafeText(history.FinalistTeamName)
            + " | MVP " + SafeText(mvpName)
            + " | Top scorer " + SafeText(history.TopScorerPlayerName) + " " + history.TopScorerPoints + "P"
            + " | Awards " + awardsCount
            + " | Records tracked " + recordsCount
            + " | User " + SafeText(history.UserTeamResult);
    }

    private static AwardWinnerData FindAward(SeasonAwardsData awards, string awardType)
    {
        if (awards == null || awards.Awards == null)
        {
            return null;
        }

        foreach (AwardWinnerData award in awards.Awards)
        {
            if (award != null && award.AwardType == awardType)
            {
                return award;
            }
        }

        return null;
    }

    private static string SafeText(string value)
    {
        return string.IsNullOrEmpty(value) ? "нет данных" : value;
    }

    private static string FormatSeason(int startYear, int endYear)
    {
        return startYear + "-" + (endYear % 100).ToString("D2");
    }

    private static string FormatMoney(int value)
    {
        return value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
    }
}
