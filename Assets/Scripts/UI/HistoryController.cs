using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HistoryController : MonoBehaviour
{
    [SerializeField] private Text _summaryText;
    [SerializeField] private Transform _seasonHistoryContainer;
    [SerializeField] private Transform _awardsContainer;
    [SerializeField] private Transform _recordsContainer;
    [SerializeField] private Transform _userTeamHistoryContainer;
    [SerializeField] private Transform _retiredPlayersContainer;
    [SerializeField] private Transform _hallOfFameContainer;
    [SerializeField] private Transform _retiredNumbersContainer;
    [SerializeField] private SeasonHistoryRowView _seasonHistoryRowPrefab;
    [SerializeField] private AwardWinnerRowView _awardWinnerRowPrefab;
    [SerializeField] private LeagueRecordRowView _recordRowPrefab;
    [SerializeField] private UserTeamHistoryRowView _userTeamHistoryRowPrefab;
    [SerializeField] private RetiredPlayerRowView _retiredPlayerRowPrefab;
    [SerializeField] private HallOfFameRowView _hallOfFameRowPrefab;
    [SerializeField] private RetiredNumberRowView _retiredNumberRowPrefab;

    public void Configure(
        Text summaryText,
        Transform seasonHistoryContainer,
        Transform awardsContainer,
        Transform recordsContainer,
        Transform userTeamHistoryContainer,
        SeasonHistoryRowView seasonHistoryRowPrefab,
        AwardWinnerRowView awardWinnerRowPrefab,
        LeagueRecordRowView recordRowPrefab,
        UserTeamHistoryRowView userTeamHistoryRowPrefab)
    {
        _summaryText = summaryText;
        _seasonHistoryContainer = seasonHistoryContainer;
        _awardsContainer = awardsContainer;
        _recordsContainer = recordsContainer;
        _userTeamHistoryContainer = userTeamHistoryContainer;
        _seasonHistoryRowPrefab = seasonHistoryRowPrefab;
        _awardWinnerRowPrefab = awardWinnerRowPrefab;
        _recordRowPrefab = recordRowPrefab;
        _userTeamHistoryRowPrefab = userTeamHistoryRowPrefab;
    }

    public void Configure(
        Text summaryText,
        Transform seasonHistoryContainer,
        Transform awardsContainer,
        Transform recordsContainer,
        Transform userTeamHistoryContainer,
        Transform retiredPlayersContainer,
        Transform hallOfFameContainer,
        Transform retiredNumbersContainer,
        SeasonHistoryRowView seasonHistoryRowPrefab,
        AwardWinnerRowView awardWinnerRowPrefab,
        LeagueRecordRowView recordRowPrefab,
        UserTeamHistoryRowView userTeamHistoryRowPrefab,
        RetiredPlayerRowView retiredPlayerRowPrefab,
        HallOfFameRowView hallOfFameRowPrefab,
        RetiredNumberRowView retiredNumberRowPrefab)
    {
        Configure(
            summaryText,
            seasonHistoryContainer,
            awardsContainer,
            recordsContainer,
            userTeamHistoryContainer,
            seasonHistoryRowPrefab,
            awardWinnerRowPrefab,
            recordRowPrefab,
            userTeamHistoryRowPrefab);

        _retiredPlayersContainer = retiredPlayersContainer;
        _hallOfFameContainer = hallOfFameContainer;
        _retiredNumbersContainer = retiredNumbersContainer;
        _retiredPlayerRowPrefab = retiredPlayerRowPrefab;
        _hallOfFameRowPrefab = hallOfFameRowPrefab;
        _retiredNumberRowPrefab = retiredNumberRowPrefab;
    }

    public void ShowHistory(GameState state)
    {
        if (_summaryText == null
            || _seasonHistoryContainer == null
            || _awardsContainer == null
            || _recordsContainer == null
            || _userTeamHistoryContainer == null
            || _seasonHistoryRowPrefab == null
            || _awardWinnerRowPrefab == null
            || _recordRowPrefab == null
            || _userTeamHistoryRowPrefab == null)
        {
            Debug.LogError("HistoryController: UI references are not configured.");
            return;
        }

        LeagueHistoryService.EnsureLeagueHistory(state);
        RetirementService.EnsureRetirementData(state);
        RenderSummary(state);
        RenderLeagueHistory(GameSession.GetLeagueHistory());
        RenderAwards(GameSession.GetAllAwardsHistory());
        RenderRecords(GameSession.GetLeagueRecords());
        RenderUserTeamHistory(GameSession.GetUserTeamHistory());
        RenderRetiredPlayers(state == null ? null : state.RetiredPlayers);
        RenderHallOfFame(state == null ? null : state.HallOfFame);
        RenderRetiredNumbers(state == null ? null : state.LeagueRetiredNumbers);
    }

    private void RenderSummary(GameState state)
    {
        LeagueSeasonHistoryData lastHistory = state == null ? null : state.LastLeagueSeasonHistory;
        SeasonAwardsData lastAwards = state == null ? null : state.LastSeasonAwards;
        AwardWinnerData mvp = FindAward(lastAwards, AwardsConfig.LeagueMvp);

        if (lastHistory == null)
        {
            _summaryText.text = "История появится после завершения первого сезона. Завершите сезон, плей-офф и драфт, затем перейдите в следующий сезон."
                + "\n" + BuildRetirementSummary(state);
            return;
        }

        _summaryText.text = FormatSeason(lastHistory.SeasonStartYear, lastHistory.SeasonEndYear)
            + " | Чемпион: " + SafeText(lastHistory.ChampionTeamName)
            + " | Финалист: " + SafeText(lastHistory.FinalistTeamName)
            + "\nЛучшая регулярка: " + SafeText(lastHistory.BestRegularSeasonTeamName)
            + " (" + lastHistory.BestRegularSeasonPoints + " pts)"
            + " | Top scorer: " + SafeText(lastHistory.TopScorerPlayerName)
            + " " + lastHistory.TopScorerPoints + "P"
            + "\nMVP: " + (mvp == null ? SafeText(lastHistory.MvpPlayerName) : SafeText(mvp.PlayerName))
            + " | Ваша команда: " + SafeText(lastHistory.UserTeamResult)
            + "\nRecap news: " + GetLatestRecapNewsTitle(state)
            + "\n" + BuildRetirementSummary(state);
    }

    private void RenderLeagueHistory(List<LeagueSeasonHistoryData> history)
    {
        ClearRows(_seasonHistoryContainer, _seasonHistoryRowPrefab.transform);
        _seasonHistoryRowPrefab.gameObject.SetActive(false);

        if (history == null || history.Count == 0)
        {
            CreateInfoRow(_seasonHistoryContainer, "История сезонов пока пуста", 52f);
            return;
        }

        int shown = 0;
        foreach (LeagueSeasonHistoryData item in history)
        {
            if (item == null)
            {
                continue;
            }

            SeasonHistoryRowView row = Instantiate(_seasonHistoryRowPrefab, _seasonHistoryContainer);
            row.name = "league-history-" + item.SeasonStartYear;
            row.gameObject.SetActive(true);
            row.Initialize(item);
            shown++;
            if (shown >= 12)
            {
                break;
            }
        }
    }

    private void RenderAwards(List<AwardWinnerData> awards)
    {
        ClearRows(_awardsContainer, _awardWinnerRowPrefab.transform);
        _awardWinnerRowPrefab.gameObject.SetActive(false);

        if (awards == null || awards.Count == 0)
        {
            CreateInfoRow(_awardsContainer, "Награды пока не вручались", 44f);
            return;
        }

        int shown = 0;
        foreach (AwardWinnerData award in awards)
        {
            if (award == null)
            {
                continue;
            }

            AwardWinnerRowView row = Instantiate(_awardWinnerRowPrefab, _awardsContainer);
            row.name = "award-" + award.AwardId;
            row.gameObject.SetActive(true);
            row.Initialize(award);
            shown++;
            if (shown >= 16)
            {
                break;
            }
        }
    }

    private void RenderRecords(LeagueRecordsData records)
    {
        ClearRows(_recordsContainer, _recordRowPrefab.transform);
        _recordRowPrefab.gameObject.SetActive(false);

        if (records == null || records.Records == null || records.Records.Count == 0)
        {
            CreateInfoRow(_recordsContainer, "Рекорды появятся после завершения сезона", 44f);
            return;
        }

        records.EnsureRecords();
        int shown = UiDisplayLimitConfig.ClampRowCount(records.Records.Count, UiDisplayLimitConfig.MaxRecordRows);
        for (int i = 0; i < shown; i++)
        {
            LeagueRecordData record = records.Records[i];
            if (record == null)
            {
                continue;
            }

            LeagueRecordRowView row = Instantiate(_recordRowPrefab, _recordsContainer);
            row.name = "record-" + record.RecordType;
            row.gameObject.SetActive(true);
            row.Initialize(record);
        }

        string limitMessage = UiDisplayLimitConfig.BuildLimitMessage(shown, records.Records.Count);
        if (!string.IsNullOrEmpty(limitMessage))
        {
            CreateInfoRow(_recordsContainer, limitMessage, 44f);
        }
    }

    private void RenderUserTeamHistory(List<UserTeamSeasonHistoryData> history)
    {
        ClearRows(_userTeamHistoryContainer, _userTeamHistoryRowPrefab.transform);
        _userTeamHistoryRowPrefab.gameObject.SetActive(false);

        if (history == null || history.Count == 0)
        {
            CreateInfoRow(_userTeamHistoryContainer, "История вашей команды пока пуста", 44f);
            return;
        }

        int shown = 0;
        foreach (UserTeamSeasonHistoryData item in history)
        {
            if (item == null)
            {
                continue;
            }

            UserTeamHistoryRowView row = Instantiate(_userTeamHistoryRowPrefab, _userTeamHistoryContainer);
            row.name = "user-team-history-" + item.SeasonStartYear;
            row.gameObject.SetActive(true);
            row.Initialize(item);
            shown++;
            if (shown >= 12)
            {
                break;
            }
        }
    }

    private void RenderRetiredPlayers(RetiredPlayersData retiredPlayers)
    {
        if (_retiredPlayersContainer == null || _retiredPlayerRowPrefab == null)
        {
            return;
        }

        ClearRows(_retiredPlayersContainer, _retiredPlayerRowPrefab.transform);
        _retiredPlayerRowPrefab.gameObject.SetActive(false);

        if (retiredPlayers == null || retiredPlayers.Players == null || retiredPlayers.Players.Count == 0)
        {
            CreateInfoRow(_retiredPlayersContainer, "Завершивших карьеру пока нет", 48f);
            return;
        }

        List<RetiredPlayerData> players = new List<RetiredPlayerData>(retiredPlayers.Players);
        players.Sort(CompareRetiredPlayers);

        int shown = 0;
        foreach (RetiredPlayerData player in players)
        {
            if (player == null)
            {
                continue;
            }

            RetiredPlayerRowView row = Instantiate(_retiredPlayerRowPrefab, _retiredPlayersContainer);
            row.name = "retired-player-" + player.PlayerId;
            row.gameObject.SetActive(true);
            row.Initialize(player);
            shown++;
            if (shown >= 10)
            {
                break;
            }
        }
    }

    private void RenderHallOfFame(HallOfFameData hallOfFame)
    {
        if (_hallOfFameContainer == null || _hallOfFameRowPrefab == null)
        {
            return;
        }

        ClearRows(_hallOfFameContainer, _hallOfFameRowPrefab.transform);
        _hallOfFameRowPrefab.gameObject.SetActive(false);

        if (hallOfFame == null || hallOfFame.Inductees == null || hallOfFame.Inductees.Count == 0)
        {
            CreateInfoRow(_hallOfFameContainer, "Hall of Fame пока пуст", 48f);
            return;
        }

        List<HallOfFameInducteeData> inductees = new List<HallOfFameInducteeData>(hallOfFame.Inductees);
        inductees.Sort(CompareHallOfFameInductees);

        int shown = 0;
        foreach (HallOfFameInducteeData inductee in inductees)
        {
            if (inductee == null)
            {
                continue;
            }

            HallOfFameRowView row = Instantiate(_hallOfFameRowPrefab, _hallOfFameContainer);
            row.name = "hall-of-fame-" + inductee.PlayerId;
            row.gameObject.SetActive(true);
            row.Initialize(inductee);
            shown++;
            if (shown >= 10)
            {
                break;
            }
        }
    }

    private void RenderRetiredNumbers(List<RetiredNumberData> retiredNumbers)
    {
        if (_retiredNumbersContainer == null || _retiredNumberRowPrefab == null)
        {
            return;
        }

        ClearRows(_retiredNumbersContainer, _retiredNumberRowPrefab.transform);
        _retiredNumberRowPrefab.gameObject.SetActive(false);

        if (retiredNumbers == null || retiredNumbers.Count == 0)
        {
            CreateInfoRow(_retiredNumbersContainer, "Выведенных номеров пока нет", 48f);
            return;
        }

        List<RetiredNumberData> numbers = new List<RetiredNumberData>(retiredNumbers);
        numbers.Sort(CompareRetiredNumbers);

        int shown = 0;
        foreach (RetiredNumberData retiredNumber in numbers)
        {
            if (retiredNumber == null)
            {
                continue;
            }

            RetiredNumberRowView row = Instantiate(_retiredNumberRowPrefab, _retiredNumbersContainer);
            row.name = "retired-number-" + retiredNumber.TeamId + "-" + retiredNumber.JerseyNumber;
            row.gameObject.SetActive(true);
            row.Initialize(retiredNumber);
            shown++;
            if (shown >= 10)
            {
                break;
            }
        }
    }

    private static string BuildRetirementSummary(GameState state)
    {
        int retiredCount = state == null || state.RetiredPlayers == null || state.RetiredPlayers.Players == null
            ? 0
            : state.RetiredPlayers.Players.Count;
        int hallOfFameCount = state == null || state.HallOfFame == null || state.HallOfFame.Inductees == null
            ? 0
            : state.HallOfFame.Inductees.Count;
        int retiredNumberCount = state == null || state.LeagueRetiredNumbers == null
            ? 0
            : state.LeagueRetiredNumbers.Count;

        return "Retired: " + retiredCount
            + " | Hall of Fame: " + hallOfFameCount
            + " | Retired numbers: " + retiredNumberCount;
    }

    private static int CompareRetiredPlayers(RetiredPlayerData left, RetiredPlayerData right)
    {
        if (left == null && right == null)
        {
            return 0;
        }

        if (left == null)
        {
            return 1;
        }

        if (right == null)
        {
            return -1;
        }

        int seasonCompare = right.RetirementSeasonEndYear.CompareTo(left.RetirementSeasonEndYear);
        if (seasonCompare != 0)
        {
            return seasonCompare;
        }

        int scoreCompare = right.HallOfFameScore.CompareTo(left.HallOfFameScore);
        return scoreCompare != 0 ? scoreCompare : right.CareerPoints.CompareTo(left.CareerPoints);
    }

    private static int CompareHallOfFameInductees(HallOfFameInducteeData left, HallOfFameInducteeData right)
    {
        if (left == null && right == null)
        {
            return 0;
        }

        if (left == null)
        {
            return 1;
        }

        if (right == null)
        {
            return -1;
        }

        int yearCompare = right.InductionYear.CompareTo(left.InductionYear);
        return yearCompare != 0 ? yearCompare : right.HallOfFameScore.CompareTo(left.HallOfFameScore);
    }

    private static int CompareRetiredNumbers(RetiredNumberData left, RetiredNumberData right)
    {
        if (left == null && right == null)
        {
            return 0;
        }

        if (left == null)
        {
            return 1;
        }

        if (right == null)
        {
            return -1;
        }

        int yearCompare = right.RetiredNumberYear.CompareTo(left.RetiredNumberYear);
        return yearCompare != 0 ? yearCompare : right.RetiredNumberScore.CompareTo(left.RetiredNumberScore);
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

    private static string GetLatestRecapNewsTitle(GameState state)
    {
        List<NewsItemData> recapNews = NewsFeedService.GetNewsByCategory(state, NewsConfig.CategorySeasonRecap, 1);
        if (recapNews == null || recapNews.Count == 0 || recapNews[0] == null)
        {
            return "нет";
        }

        return SafeText(recapNews[0].Title);
    }

    private static void ClearRows(Transform container, Transform template)
    {
        if (container == null)
        {
            return;
        }

        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Transform child = container.GetChild(i);
            if (child == template)
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }

    private static void CreateInfoRow(Transform container, string value, float height)
    {
        GameObject rowObject = new GameObject("InfoRow");
        rowObject.transform.SetParent(container, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, height);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = height;
        layoutElement.minHeight = height;

        Text text = rowObject.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 15;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
    }

    private static string FormatSeason(int startYear, int endYear)
    {
        return startYear + "-" + (endYear % 100).ToString("D2");
    }

    private static string SafeText(string value)
    {
        return string.IsNullOrEmpty(value) ? "нет данных" : value;
    }
}
