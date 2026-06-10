using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DefensePairRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(DefensePairData pair, TeamData team)
    {
        if (_infoText == null)
        {
            return;
        }

        if (pair == null)
        {
            _infoText.text = "Пара недоступна";
            return;
        }

        List<PlayerData> players = new List<PlayerData>();
        PlayerData left = FindPlayer(team, pair.LeftDefensePlayerId);
        PlayerData right = FindPlayer(team, pair.RightDefensePlayerId);
        AddPlayer(players, left);
        AddPlayer(players, right);

        _infoText.text = "Пара " + pair.PairNumber
            + " | LD: " + FormatPlayer(left)
            + " | RD: " + FormatPlayer(right)
            + " | AVG " + AverageEffectiveOverall(players);
    }

    private static string FormatPlayer(PlayerData player)
    {
        if (player == null)
        {
            return "пусто";
        }

        PlayerFatigueService.EnsureFatigueFields(player);
        InjuryService.EnsureInjuryFields(player);
        PlayerRoleService.EnsureRole(player);
        return player.FirstName + " " + player.LastName
            + " (" + player.Position
            + " OVR " + player.Overall
            + " EFF " + PlayerFatigueService.GetEffectiveOverall(player)
            + " " + player.PlayerRole
            + " TOI " + IceTimeConfig.FormatSeconds(player.EstimatedTimeOnIceSeconds)
            + " COND " + player.Condition
            + (player.IsInjured ? " INJ " + player.InjuryDaysRemaining + "д" : "") + ")";
    }

    private static int AverageEffectiveOverall(List<PlayerData> players)
    {
        if (players == null || players.Count == 0)
        {
            return 0;
        }

        int total = 0;
        int count = 0;
        foreach (PlayerData player in players)
        {
            if (!InjuryService.IsPlayerAvailable(player))
            {
                continue;
            }

            total += PlayerFatigueService.GetEffectiveOverall(player);
            count++;
        }

        return count == 0 ? 0 : Mathf.RoundToInt((float)total / count);
    }

    private static void AddPlayer(List<PlayerData> players, PlayerData player)
    {
        if (player != null)
        {
            players.Add(player);
        }
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
}
