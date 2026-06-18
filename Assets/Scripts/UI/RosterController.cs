using UnityEngine;
using UnityEngine.UI;

public class RosterController : MonoBehaviour
{
    [SerializeField] private Text _summaryText;
    [SerializeField] private Transform _playersContainer;
    [SerializeField] private PlayerRowView _playerRowPrefab;

    public void Configure(Transform playersContainer, PlayerRowView playerRowPrefab)
    {
        _playersContainer = playersContainer;
        _playerRowPrefab = playerRowPrefab;
    }

    public void Configure(Text summaryText, Transform playersContainer, PlayerRowView playerRowPrefab)
    {
        _summaryText = summaryText;
        _playersContainer = playersContainer;
        _playerRowPrefab = playerRowPrefab;
    }

    public void ShowRoster()
    {
        if (_playersContainer == null || _playerRowPrefab == null)
        {
            Debug.LogError("RosterController: UI references are not configured.");
            return;
        }

        ClearRows();
        _playerRowPrefab.gameObject.SetActive(false);

        TeamData team = GameSession.CurrentTeam;
        if (team == null)
        {
            Debug.LogWarning("RosterController: team is not selected.");
            return;
        }

        team.EnsurePlayers();
        TeamRosterService.EnsureRosterStatusesForTeam(team);
        if (team.Players.Count == 0)
        {
            team.Players = PlayerSeedData.CreatePlayersForTeam(team.Id);
        }

        PlayerFatigueService.EnsureFatigueForTeam(team);
        InjuryService.EnsureInjuryFieldsForTeam(team);
        LineupService.EnsureLineup(team);
        IceTimeService.EnsureUsageForTeam(team);
        LeadershipService.EnsureLeadershipForTeam(team);
        UpdateSummary(team);
        int shown = UiDisplayLimitConfig.ClampRowCount(team.Players.Count, UiDisplayLimitConfig.MaxRosterRows);
        for (int i = 0; i < shown; i++)
        {
            PlayerData player = team.Players[i];
            PlayerRowView row = Instantiate(_playerRowPrefab, _playersContainer);
            row.name = player.Id + "-row";
            row.gameObject.SetActive(true);
            row.Initialize(player, team);
        }

        AppendLimitMessage(shown, team.Players.Count);
    }

    private void UpdateSummary(TeamData team)
    {
        if (_summaryText == null || team == null)
        {
            return;
        }

        TeamRosterSummaryData roster = TeamRosterService.GetRosterSummary(team);
        TeamFinanceData finance = SalaryCapService.CalculateTeamFinance(team);
        TeamMoraleSummaryData morale = MoraleService.BuildTeamMoraleSummary(GameSession.CurrentState, team);
        int injured = GameSession.GetCurrentTeamInjuredPlayers().Count;
        _summaryText.text = "Pro " + roster.NhlPlayers + "/" + RosterStatusConfig.MaxNhlRosterSize
            + " | Farm " + roster.FarmPlayers
            + " | Reserve " + roster.ReservePlayers
            + " | Inj " + injured
            + "\nAvg morale " + (morale == null ? 0 : morale.AverageMorale)
            + " | Payroll " + MobileUiConfig.FormatMoney(finance == null ? 0 : finance.Payroll)
            + " | Cap " + MobileUiConfig.FormatMoney(finance == null ? 0 : finance.CapSpace);
    }

    private void AppendLimitMessage(int shown, int total)
    {
        if (_summaryText == null)
        {
            return;
        }

        string message = UiDisplayLimitConfig.BuildLimitMessage(shown, total);
        if (!string.IsNullOrEmpty(message))
        {
            _summaryText.text += "\n" + message;
        }
    }

    private void ClearRows()
    {
        for (int i = _playersContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = _playersContainer.GetChild(i);
            if (child == _playerRowPrefab.transform)
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }
}
