using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RolesController : MonoBehaviour
{
    [SerializeField] private Text _summaryText;
    [SerializeField] private Text _selectedPlayerText;
    [SerializeField] private Transform _playersContainer;
    [SerializeField] private RolePlayerRowView _playerRowPrefab;
    [SerializeField] private GameScreenController _screenController;

    public void Configure(
        Text summaryText,
        Text selectedPlayerText,
        Transform playersContainer,
        RolePlayerRowView playerRowPrefab,
        GameScreenController screenController)
    {
        _summaryText = summaryText;
        _selectedPlayerText = selectedPlayerText;
        _playersContainer = playersContainer;
        _playerRowPrefab = playerRowPrefab;
        _screenController = screenController;
    }

    public void ShowRoles(GameState state, string selectedPlayerId)
    {
        if (_summaryText == null || _selectedPlayerText == null || _playersContainer == null || _playerRowPrefab == null)
        {
            Debug.LogError("RolesController: UI references are not configured.");
            return;
        }

        TeamData team = FindTeam(state, state == null ? "" : state.SelectedTeamId);
        if (team == null)
        {
            _summaryText.text = "Команда не выбрана";
            _selectedPlayerText.text = "Выберите игрока для изменения роли";
            ClearRows();
            return;
        }

        IceTimeService.EnsureUsageForTeam(team);
        RenderSummary(team);
        RenderSelectedPlayer(team, selectedPlayerId);
        RenderPlayers(team);
    }

    private void RenderSummary(TeamData team)
    {
        TeamUsageSummaryData summary = IceTimeService.CalculateTeamUsageSummary(team);
        PlayerData topForward = FindTopPlayerByToi(team, "F");
        PlayerData topDefense = FindTopPlayerByToi(team, "D");
        PlayerData starter = LineupService.GetStartingGoalie(team);

        _summaryText.text = TeamIdentityService.GetDisplayName(team)
            + "\nСреднее TOI активных: " + IceTimeConfig.FormatSeconds(summary.AverageActiveTimeOnIceSeconds)
            + " | Активные: " + summary.ActivePlayerCount
            + " | Запасные: " + summary.ScratchPlayerCount
            + "\nТоп форвард: " + FormatPlayerSummary(topForward)
            + " | Топ защитник: " + FormatPlayerSummary(topDefense)
            + "\nСтартовый вратарь: " + FormatPlayerSummary(starter);
    }

    private void RenderSelectedPlayer(TeamData team, string selectedPlayerId)
    {
        PlayerData player = FindPlayer(team, selectedPlayerId);
        if (player == null)
        {
            _selectedPlayerText.text = "Выберите игрока для изменения роли";
            return;
        }

        PlayerRoleService.EnsureRole(player);
        PlayerFatigueService.EnsureFatigueFields(player);
        InjuryService.EnsureInjuryFields(player);

        _selectedPlayerText.text = "Выбран: " + player.FirstName + " " + player.LastName
            + " | " + player.Position
            + " | OVR " + player.Overall
            + " | POT " + player.Potential
            + "\nРоль: " + player.PlayerRole
            + (player.IsRoleManual ? " (manual)" : " (auto)")
            + " | Usage: " + player.UsageCategory
            + " | Expected TOI: " + IceTimeConfig.FormatSeconds(player.EstimatedTimeOnIceSeconds)
            + "\nATOI: " + IceTimeConfig.FormatSeconds(player.AverageTimeOnIceSeconds)
            + " | Last: " + IceTimeConfig.FormatSeconds(player.LastGameTimeOnIceSeconds)
            + " | COND " + player.Condition
            + " | FAT " + player.Fatigue
            + (player.IsInjured ? " | INJ " + player.InjuryDaysRemaining + " дн." : "");
    }

    private void RenderPlayers(TeamData team)
    {
        ClearRows();
        _playerRowPrefab.gameObject.SetActive(false);

        List<PlayerUsageData> usageList = IceTimeService.CalculateTeamUsage(team);
        foreach (PlayerUsageData usage in usageList)
        {
            RolePlayerRowView row = Instantiate(_playerRowPrefab, _playersContainer);
            row.gameObject.SetActive(true);
            row.Initialize(usage, _screenController);
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

    private static TeamData FindTeam(GameState state, string teamId)
    {
        if (state == null || state.Teams == null)
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

    private static PlayerData FindPlayer(TeamData team, string playerId)
    {
        if (team == null || string.IsNullOrEmpty(playerId))
        {
            return null;
        }

        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            if (player != null && player.Id == playerId)
            {
                return player;
            }
        }

        return null;
    }

    private static PlayerData FindTopPlayerByToi(TeamData team, string category)
    {
        PlayerData topPlayer = null;
        if (team == null)
        {
            return null;
        }

        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            if (player == null)
            {
                continue;
            }

            bool matchesCategory = category == "D"
                ? player.Position == "D"
                : player.Position == "C" || player.Position == "LW" || player.Position == "RW";
            if (!matchesCategory)
            {
                continue;
            }

            if (topPlayer == null || player.EstimatedTimeOnIceSeconds > topPlayer.EstimatedTimeOnIceSeconds)
            {
                topPlayer = player;
            }
        }

        return topPlayer;
    }

    private static string FormatPlayerSummary(PlayerData player)
    {
        if (player == null)
        {
            return "нет данных";
        }

        return player.FirstName + " " + player.LastName
            + " " + IceTimeConfig.FormatSeconds(player.EstimatedTimeOnIceSeconds);
    }
}
