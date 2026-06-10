using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ForwardLineRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(ForwardLineData line, TeamData team)
    {
        if (_infoText == null)
        {
            return;
        }

        if (line == null)
        {
            _infoText.text = "Звено недоступно";
            return;
        }

        List<PlayerData> players = new List<PlayerData>();
        PlayerData lw = FindPlayer(team, line.LeftWingPlayerId);
        PlayerData c = FindPlayer(team, line.CenterPlayerId);
        PlayerData rw = FindPlayer(team, line.RightWingPlayerId);
        AddPlayer(players, lw);
        AddPlayer(players, c);
        AddPlayer(players, rw);

        _infoText.text = "Звено " + line.LineNumber
            + " | LW: " + FormatPlayer(lw)
            + " | C: " + FormatPlayer(c)
            + " | RW: " + FormatPlayer(rw)
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
