using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class ProspectRightsController : MonoBehaviour
{
    [SerializeField] private Text _teamInfoText;
    [SerializeField] private Text _selectedProspectText;
    [SerializeField] private Transform _rightsContainer;
    [SerializeField] private Transform _historyContainer;
    [SerializeField] private ProspectRightsRowView _rightsRowPrefab;
    [SerializeField] private ProspectSigningHistoryRowView _historyRowPrefab;
    [SerializeField] private GameScreenController _screenController;

    public void Configure(
        Text teamInfoText,
        Text selectedProspectText,
        Transform rightsContainer,
        Transform historyContainer,
        ProspectRightsRowView rightsRowPrefab,
        ProspectSigningHistoryRowView historyRowPrefab,
        GameScreenController screenController)
    {
        _teamInfoText = teamInfoText;
        _selectedProspectText = selectedProspectText;
        _rightsContainer = rightsContainer;
        _historyContainer = historyContainer;
        _rightsRowPrefab = rightsRowPrefab;
        _historyRowPrefab = historyRowPrefab;
        _screenController = screenController;
    }

    public void ShowProspectRights(GameState state, string selectedProspectId)
    {
        if (!HasRequiredReferences())
        {
            Debug.LogError("ProspectRightsController: UI references are not configured.");
            return;
        }

        ProspectSigningService.EnsureProspectSigningHistory(state);
        RenderTeamInfo(state);
        RenderSelectedProspect(state, selectedProspectId);
        RenderRights(state);
        RenderHistory(state);
    }

    private void RenderTeamInfo(GameState state)
    {
        TeamData team = GetUserTeam(state);
        if (team == null)
        {
            _teamInfoText.text = "Команда не найдена";
            return;
        }

        team.EnsurePlayers();
        team.EnsureDraftRights();
        TeamFinanceData finance = SalaryCapService.CalculateTeamFinance(team);
        _teamInfoText.text = TeamIdentityService.GetDisplayName(team)
            + "\nПрава на проспектов: " + team.DraftRights.Count
            + "\nPayroll: " + FormatMoney(finance.Payroll)
            + " | Cap space: " + FormatMoney(finance.CapSpace)
            + "\nСостав: " + finance.PlayerCount + " / " + SalaryCapConfig.MaxRosterSize;
    }

    private void RenderSelectedProspect(GameState state, string selectedProspectId)
    {
        TeamData team = GetUserTeam(state);
        ProspectData prospect = team == null
            ? null
            : ProspectSigningService.FindProspectRights(state, team.Id, selectedProspectId);

        if (prospect == null)
        {
            _selectedProspectText.text = "Выбранный проспект: не выбран";
            return;
        }

        int elcYears = EntryLevelContractConfig.GetContractYearsByAge(prospect.Age);
        string elcLine = elcYears <= 0
            ? "Игрок не подходит для ELC"
            : "ELC: " + elcYears + " г. | $" + FormatMoney(CalculateEstimatedElcSalary(prospect, GetRules(state)));
        int rank = ScoutingService.GetProspectRank(prospect, prospect.DraftPickOverall);
        ScoutingService.EnsureProspectScouting(prospect, rank);

        _selectedProspectText.text = prospect.FirstName + " " + prospect.LastName
            + " | #" + rank
            + " | " + prospect.ProjectedRound
            + " | " + prospect.ProspectArchetype
            + " | " + prospect.Position
            + " | " + prospect.Age
            + " | OVR " + prospect.Overall
            + " | POT " + prospect.Potential
            + " | " + prospect.ScoutingGrade
            + " | " + prospect.ProjectedRole
            + " | " + prospect.RiskHint
            + " | " + prospect.DevelopmentTypeHint
            + " | " + prospect.CeilingHint
            + " | " + prospect.FloorHint
            + " | " + FormatDevelopment(prospect.LastDevelopmentDelta)
            + " | R" + prospect.DraftRound
            + " | #" + prospect.DraftPickOverall
            + "\n" + elcLine;
    }

    private void RenderRights(GameState state)
    {
        ClearRows(_rightsContainer, _rightsRowPrefab.transform);
        _rightsRowPrefab.gameObject.SetActive(false);

        TeamData team = GetUserTeam(state);
        if (team == null)
        {
            CreateInfoRow(_rightsContainer, "Команда не найдена");
            return;
        }

        List<ProspectData> rights = ProspectSigningService.GetTeamDraftRights(state, team.Id);
        if (rights.Count == 0)
        {
            CreateInfoRow(_rightsContainer, "У команды нет прав на проспектов");
            return;
        }

        foreach (ProspectData prospect in rights)
        {
            ProspectRightsRowView row = Instantiate(_rightsRowPrefab, _rightsContainer);
            row.name = prospect.Id + "-prospect-rights-row";
            row.gameObject.SetActive(true);
            row.Initialize(prospect, _screenController);
        }
    }

    private void RenderHistory(GameState state)
    {
        ClearRows(_historyContainer, _historyRowPrefab.transform);
        _historyRowPrefab.gameObject.SetActive(false);

        if (state == null
            || state.ProspectSigningHistory == null
            || state.ProspectSigningHistory.Signings == null
            || state.ProspectSigningHistory.Signings.Count == 0)
        {
            CreateInfoRow(_historyContainer, "История подписаний пуста");
            return;
        }

        int firstIndex = Mathf.Max(0, state.ProspectSigningHistory.Signings.Count - 10);
        for (int i = state.ProspectSigningHistory.Signings.Count - 1; i >= firstIndex; i--)
        {
            ProspectSigningHistoryRowView row = Instantiate(_historyRowPrefab, _historyContainer);
            row.name = "prospect-signing-" + i;
            row.gameObject.SetActive(true);
            row.Initialize(state.ProspectSigningHistory.Signings[i]);
        }
    }

    private bool HasRequiredReferences()
    {
        return _teamInfoText != null
            && _selectedProspectText != null
            && _rightsContainer != null
            && _historyContainer != null
            && _rightsRowPrefab != null
            && _historyRowPrefab != null;
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

    private static LeagueRulesData GetRules(GameState state)
    {
        return state == null || state.LeagueRules == null
            ? LeagueRulesConfig.CreateDefaultRules()
            : state.LeagueRules;
    }

    private static int CalculateEstimatedElcSalary(ProspectData prospect, LeagueRulesData rules)
    {
        int salary = rules.LeagueMinimumSalary;
        if (prospect.DraftRound == 1)
        {
            salary += EntryLevelContractConfig.SalaryPremiumForRound1;
        }
        else if (prospect.DraftRound == 2)
        {
            salary += EntryLevelContractConfig.SalaryPremiumForRound2;
        }
        else if (prospect.DraftRound == 3)
        {
            salary += EntryLevelContractConfig.SalaryPremiumForRound3;
        }

        if (prospect.Potential >= 90)
        {
            salary += 200000;
        }
        else if (prospect.Potential >= 85)
        {
            salary += 100000;
        }

        if (salary < rules.LeagueMinimumSalary)
        {
            salary = rules.LeagueMinimumSalary;
        }

        if (salary > rules.MaximumPlayerSalary)
        {
            salary = rules.MaximumPlayerSalary;
        }

        return salary;
    }

    private static void ClearRows(Transform container, Transform prefabTransform)
    {
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Transform child = container.GetChild(i);
            if (child == prefabTransform)
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }

    private static void CreateInfoRow(Transform container, string value)
    {
        GameObject rowObject = new GameObject("InfoRow");
        rowObject.transform.SetParent(container, false);

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

    private static string FormatMoney(int value)
    {
        return value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
    }

    private static string FormatDevelopment(int value)
    {
        if (value > 0)
        {
            return "DEV +" + value;
        }

        if (value < 0)
        {
            return "DEV " + value;
        }

        return "DEV 0";
    }
}
