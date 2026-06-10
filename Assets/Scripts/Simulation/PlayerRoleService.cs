using System.Collections.Generic;
using UnityEngine;

public static class PlayerRoleService
{
    public static void EnsureRolesForTeam(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            EnsureRole(player);
        }
    }

    public static void EnsureRolesForTeams(List<TeamData> teams)
    {
        if (teams == null)
        {
            return;
        }

        foreach (TeamData team in teams)
        {
            EnsureRolesForTeam(team);
        }
    }

    public static void EnsureRole(PlayerData player)
    {
        if (player == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(player.PlayerRole))
        {
            player.PlayerRole = DetermineDefaultRole(player);
            player.IsRoleManual = false;
        }
        else if (!IsRoleValidForPosition(player.PlayerRole, player.Position))
        {
            if (player.IsRoleManual)
            {
                Debug.LogWarning("Manual role does not match player position: " + player.FirstName + " " + player.LastName);
            }
            else
            {
                player.PlayerRole = DetermineDefaultRole(player);
            }
        }

        if (string.IsNullOrEmpty(player.UsageCategory))
        {
            player.UsageCategory = "Scratch";
        }
    }

    public static string DetermineDefaultRole(PlayerData player)
    {
        if (player == null)
        {
            return "";
        }

        if (player.Position == "G")
        {
            if (player.Overall >= 82)
            {
                return PlayerRoleConfig.StarterGoalie;
            }

            if (player.Overall >= 72)
            {
                return PlayerRoleConfig.BackupGoalie;
            }

            return PlayerRoleConfig.DepthGoalie;
        }

        if (player.Position == "D")
        {
            if (player.Overall >= 82 && player.Potential >= 85)
            {
                return PlayerRoleConfig.TwoWayDefenseman;
            }

            if (player.Overall >= 78 && player.Potential >= 84)
            {
                return PlayerRoleConfig.OffensiveDefenseman;
            }

            if (player.Overall >= 75)
            {
                return PlayerRoleConfig.DefensiveDefenseman;
            }

            return PlayerRoleConfig.StayAtHomeDefenseman;
        }

        if (player.Overall >= 84)
        {
            return player.Potential >= 88 ? PlayerRoleConfig.Playmaker : PlayerRoleConfig.Sniper;
        }

        if (player.Overall >= 78)
        {
            return PlayerRoleConfig.TwoWayForward;
        }

        if (player.Overall >= 72)
        {
            return PlayerRoleConfig.PowerForward;
        }

        if (player.Overall >= 66)
        {
            return PlayerRoleConfig.Grinder;
        }

        return PlayerRoleConfig.DepthForward;
    }

    public static bool SetPlayerRole(PlayerData player, string role, bool manual, out string message)
    {
        if (player == null)
        {
            message = "Игрок не найден";
            return false;
        }

        if (!IsRoleValidForPosition(role, player.Position))
        {
            message = "Роль не подходит позиции игрока";
            return false;
        }

        player.PlayerRole = role;
        player.IsRoleManual = manual;
        message = "Роль изменена";
        return true;
    }

    public static float GetGoalScoringModifier(PlayerData player)
    {
        return GetModifier(player, 0);
    }

    public static float GetAssistModifier(PlayerData player)
    {
        return GetModifier(player, 1);
    }

    public static float GetShotModifier(PlayerData player)
    {
        return GetModifier(player, 2);
    }

    public static float GetPenaltyModifier(PlayerData player)
    {
        return GetModifier(player, 3);
    }

    public static float GetDefensiveModifier(PlayerData player)
    {
        return GetModifier(player, 4);
    }

    private static bool IsRoleValidForPosition(string role, string position)
    {
        if (position == "C" || position == "LW" || position == "RW")
        {
            return PlayerRoleConfig.IsForwardRole(role);
        }

        if (position == "D")
        {
            return PlayerRoleConfig.IsDefenseRole(role);
        }

        if (position == "G")
        {
            return PlayerRoleConfig.IsGoalieRole(role);
        }

        return false;
    }

    private static float GetModifier(PlayerData player, int index)
    {
        EnsureRole(player);
        string role = player == null ? "" : player.PlayerRole;
        if (role == PlayerRoleConfig.Sniper)
        {
            return Pick(index, 1.20f, 0.95f, 1.15f, 1.00f, 0.95f);
        }

        if (role == PlayerRoleConfig.Playmaker)
        {
            return Pick(index, 1.00f, 1.20f, 1.00f, 0.95f, 1.00f);
        }

        if (role == PlayerRoleConfig.PowerForward)
        {
            return Pick(index, 1.08f, 1.00f, 1.08f, 1.12f, 1.00f);
        }

        if (role == PlayerRoleConfig.TwoWayForward)
        {
            return Pick(index, 1.00f, 1.05f, 1.00f, 0.95f, 1.15f);
        }

        if (role == PlayerRoleConfig.Grinder)
        {
            return Pick(index, 0.90f, 0.90f, 0.90f, 1.15f, 1.10f);
        }

        if (role == PlayerRoleConfig.DepthForward)
        {
            return Pick(index, 0.80f, 0.85f, 0.85f, 1.00f, 0.95f);
        }

        if (role == PlayerRoleConfig.OffensiveDefenseman)
        {
            return Pick(index, 1.10f, 1.15f, 1.10f, 1.00f, 0.95f);
        }

        if (role == PlayerRoleConfig.DefensiveDefenseman)
        {
            return Pick(index, 0.75f, 0.90f, 0.85f, 1.00f, 1.20f);
        }

        if (role == PlayerRoleConfig.TwoWayDefenseman)
        {
            return Pick(index, 0.95f, 1.05f, 1.00f, 0.95f, 1.10f);
        }

        if (role == PlayerRoleConfig.StayAtHomeDefenseman)
        {
            return Pick(index, 0.70f, 0.80f, 0.80f, 1.05f, 1.15f);
        }

        return 1f;
    }

    private static float Pick(int index, float goal, float assist, float shot, float penalty, float defense)
    {
        if (index == 0)
        {
            return goal;
        }

        if (index == 1)
        {
            return assist;
        }

        if (index == 2)
        {
            return shot;
        }

        if (index == 3)
        {
            return penalty;
        }

        return defense;
    }
}
