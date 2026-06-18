using System.Collections.Generic;

public static class LiveMatchLineSelectorService
{
    public static List<PlayerData> SelectSkaters(TeamData team, LiveMatchStateData match, bool goaliePulled)
    {
        if (match != null && match.IsOvertime && !match.IsPlayoffGame)
        {
            return SelectOvertimeSkaters(team, 2, 1);
        }

        if (goaliePulled)
        {
            List<PlayerData> skaters = SelectRegulationSkaters(team, match);
            PlayerData extra = FindBestExtraSkater(team, skaters);
            if (extra != null)
            {
                skaters.Add(extra);
            }

            return skaters;
        }

        return match != null && match.IsOvertime
            ? SelectOvertimeSkaters(team, 3, 2)
            : SelectRegulationSkaters(team, match);
    }

    public static PlayerData SelectShooter(TeamData team, int round)
    {
        List<PlayerData> skaters = GetAvailableSkaters(team);
        skaters.Sort(CompareSkater);
        return skaters.Count == 0 ? null : skaters[round % skaters.Count];
    }

    private static List<PlayerData> SelectRegulationSkaters(TeamData team, LiveMatchStateData match)
    {
        List<PlayerData> selected = new List<PlayerData>();
        LineupService.EnsureLineup(team);
        if (team == null || team.Lineup == null)
        {
            return selected;
        }

        int tick = match == null ? 0 : match.TotalGameSecondsElapsed / LiveMatchConfig.LiveTickGameSeconds;
        team.Lineup.EnsureCollections();
        if (team.Lineup.ForwardLines.Count > 0)
        {
            ForwardLineData line = team.Lineup.ForwardLines[tick % team.Lineup.ForwardLines.Count];
            AddAvailable(team, selected, line == null ? "" : line.LeftWingPlayerId);
            AddAvailable(team, selected, line == null ? "" : line.CenterPlayerId);
            AddAvailable(team, selected, line == null ? "" : line.RightWingPlayerId);
        }

        if (team.Lineup.DefensePairs.Count > 0)
        {
            DefensePairData pair = team.Lineup.DefensePairs[tick % team.Lineup.DefensePairs.Count];
            AddAvailable(team, selected, pair == null ? "" : pair.LeftDefensePlayerId);
            AddAvailable(team, selected, pair == null ? "" : pair.RightDefensePlayerId);
        }

        FillByPosition(team, selected, "F", 3);
        FillByPosition(team, selected, "D", 2);
        return selected;
    }

    private static List<PlayerData> SelectOvertimeSkaters(TeamData team, int forwards, int defensemen)
    {
        List<PlayerData> selected = new List<PlayerData>();
        FillByPosition(team, selected, "F", forwards);
        FillByPosition(team, selected, "D", defensemen);
        return selected;
    }

    private static PlayerData FindBestExtraSkater(TeamData team, List<PlayerData> excluded)
    {
        HashSet<string> excludedIds = new HashSet<string>();
        if (excluded != null)
        {
            foreach (PlayerData player in excluded)
            {
                if (player != null)
                {
                    excludedIds.Add(player.Id);
                }
            }
        }

        List<PlayerData> skaters = GetAvailableSkaters(team);
        skaters.Sort(CompareSkater);
        foreach (PlayerData player in skaters)
        {
            if (!excludedIds.Contains(player.Id))
            {
                return player;
            }
        }

        return null;
    }

    private static void FillByPosition(TeamData team, List<PlayerData> selected, string positionGroup, int count)
    {
        HashSet<string> selectedIds = new HashSet<string>();
        foreach (PlayerData player in selected)
        {
            if (player != null)
            {
                selectedIds.Add(player.Id);
            }
        }

        List<PlayerData> players = positionGroup == "D" ? GetAvailableDefensemen(team) : GetAvailableForwards(team);
        players.Sort(CompareSkater);
        foreach (PlayerData player in players)
        {
            if (selected.Count >= count + CountOtherGroup(selected, positionGroup))
            {
                return;
            }

            if (!selectedIds.Contains(player.Id))
            {
                selected.Add(player);
                selectedIds.Add(player.Id);
            }
        }
    }

    private static int CountOtherGroup(List<PlayerData> selected, string positionGroup)
    {
        int count = 0;
        foreach (PlayerData player in selected)
        {
            if (player == null)
            {
                continue;
            }

            bool isDefense = player.Position == "D";
            if ((positionGroup == "D" && !isDefense) || (positionGroup != "D" && isDefense))
            {
                count++;
            }
        }

        return count;
    }

    private static void AddAvailable(TeamData team, List<PlayerData> selected, string playerId)
    {
        PlayerData player = FindPlayer(team, playerId);
        if (player != null
            && RosterStatusConfig.IsNhlRoster(player)
            && !player.IsRetired
            && InjuryService.IsPlayerAvailable(player))
        {
            selected.Add(player);
        }
    }

    private static List<PlayerData> GetAvailableForwards(TeamData team)
    {
        List<PlayerData> players = GetAvailableSkaters(team);
        players.RemoveAll(player => player == null || player.Position == "D");
        return players;
    }

    private static List<PlayerData> GetAvailableDefensemen(TeamData team)
    {
        List<PlayerData> players = GetAvailableSkaters(team);
        players.RemoveAll(player => player == null || player.Position != "D");
        return players;
    }

    private static List<PlayerData> GetAvailableSkaters(TeamData team)
    {
        List<PlayerData> players = new List<PlayerData>();
        if (team == null || team.Players == null)
        {
            return players;
        }

        foreach (PlayerData player in team.Players)
        {
            if (player != null
                && RosterStatusConfig.IsNhlRoster(player)
                && !player.IsRetired
                && player.Position != "G"
                && InjuryService.IsPlayerAvailable(player))
            {
                players.Add(player);
            }
        }

        return players;
    }

    private static int CompareSkater(PlayerData left, PlayerData right)
    {
        int overall = right.Overall.CompareTo(left.Overall);
        if (overall != 0) return overall;
        int potential = right.Potential.CompareTo(left.Potential);
        if (potential != 0) return potential;
        int condition = right.Condition.CompareTo(left.Condition);
        if (condition != 0) return condition;
        return left.Age.CompareTo(right.Age);
    }

    private static PlayerData FindPlayer(TeamData team, string playerId)
    {
        if (team == null || team.Players == null || string.IsNullOrEmpty(playerId))
        {
            return null;
        }

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
