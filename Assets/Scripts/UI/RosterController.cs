using UnityEngine;

public class RosterController : MonoBehaviour
{
    [SerializeField] private Transform _playersContainer;
    [SerializeField] private PlayerRowView _playerRowPrefab;

    public void Configure(Transform playersContainer, PlayerRowView playerRowPrefab)
    {
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
        if (team.Players.Count == 0)
        {
            team.Players = PlayerSeedData.CreatePlayersForTeam(team.Id);
        }

        PlayerFatigueService.EnsureFatigueForTeam(team);
        InjuryService.EnsureInjuryFieldsForTeam(team);
        LineupService.EnsureLineup(team);
        IceTimeService.EnsureUsageForTeam(team);
        foreach (PlayerData player in team.Players)
        {
            PlayerRowView row = Instantiate(_playerRowPrefab, _playersContainer);
            row.name = player.Id + "-row";
            row.gameObject.SetActive(true);
            row.Initialize(player, team);
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
