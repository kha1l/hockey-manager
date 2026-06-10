using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerPlayUnitRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(PowerPlayUnitData unit, TeamData team)
    {
        if (_infoText == null)
        {
            return;
        }

        List<PlayerData> players = GetPlayers(unit, team);
        _infoText.text = "PP" + (unit == null ? 0 : unit.UnitNumber)
            + " | " + FormatPlayers(players)
            + " | AVG " + AverageOverall(players);
    }

    private static List<PlayerData> GetPlayers(PowerPlayUnitData unit, TeamData team)
    {
        List<PlayerData> players = new List<PlayerData>();
        if (unit == null)
        {
            return players;
        }

        Add(players, FindPlayer(team, unit.Player1Id));
        Add(players, FindPlayer(team, unit.Player2Id));
        Add(players, FindPlayer(team, unit.Player3Id));
        Add(players, FindPlayer(team, unit.Player4Id));
        Add(players, FindPlayer(team, unit.Player5Id));
        return players;
    }

    private static string FormatPlayers(List<PlayerData> players)
    {
        if (players.Count == 0)
        {
            return "пусто";
        }

        List<string> values = new List<string>();
        foreach (PlayerData player in players)
        {
            InjuryService.EnsureInjuryFields(player);
            values.Add(player.FirstName + " " + player.LastName + " " + player.Overall
                + (player.IsInjured ? " INJ" : ""));
        }

        return string.Join(", ", values.ToArray());
    }

    private static int AverageOverall(List<PlayerData> players)
    {
        if (players.Count == 0)
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

            total += player.Overall;
            count++;
        }

        return count == 0 ? 0 : Mathf.RoundToInt((float)total / count);
    }

    private static void Add(List<PlayerData> players, PlayerData player)
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
