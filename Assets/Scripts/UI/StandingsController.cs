using System.Collections.Generic;
using UnityEngine;

public class StandingsController : MonoBehaviour
{
    public const string ModeDivisions = "Divisions";
    public const string ModeConferences = "Conferences";

    [SerializeField] private Transform _standingsContainer;
    [SerializeField] private StandingRowView _standingRowPrefab;

    public void Configure(Transform standingsContainer, StandingRowView standingRowPrefab)
    {
        _standingsContainer = standingsContainer;
        _standingRowPrefab = standingRowPrefab;
    }

    public void ShowStandings(SeasonData season)
    {
        ShowStandings(season, null, ModeDivisions);
    }

    public void ShowStandings(SeasonData season, List<TeamData> teams, string mode)
    {
        if (_standingsContainer == null || _standingRowPrefab == null)
        {
            Debug.LogError("StandingsController: UI references are not configured.");
            return;
        }

        ClearRows();
        _standingRowPrefab.gameObject.SetActive(false);

        if (season == null)
        {
            StandingRowView emptyRow = CreateRow("standings-empty-row");
            emptyRow.InitializeMessage("Standings are not available yet.");
            return;
        }

        Dictionary<string, TeamData> teamsById = BuildTeamLookup(teams);
        List<TeamStandingData> standings = StandingsService.GetSortedStandings(season);
        if (mode == ModeConferences)
        {
            ShowGroupedStandings(standings, teamsById, true);
            return;
        }

        ShowGroupedStandings(standings, teamsById, false);
    }

    private void ShowGroupedStandings(
        List<TeamStandingData> standings,
        Dictionary<string, TeamData> teamsById,
        bool groupByConference)
    {
        List<string> groups = BuildGroups(standings, teamsById, groupByConference);
        foreach (string group in groups)
        {
            StandingRowView headerRow = CreateRow("standings-" + group + "-header");
            headerRow.InitializeMessage(group);

            List<TeamStandingData> groupedStandings = FilterGroup(standings, teamsById, group, groupByConference);
            groupedStandings.Sort(CompareStandings);
            for (int i = 0; i < groupedStandings.Count; i++)
            {
                TeamStandingData standing = groupedStandings[i];
                StandingRowView row = CreateRow("standing-" + group + "-" + (i + 1).ToString("00") + "-row");
                row.InitializeDetailed(i + 1, standing, FindTeam(teamsById, standing.TeamId));
            }
        }
    }

    private StandingRowView CreateRow(string rowName)
    {
        StandingRowView row = Instantiate(_standingRowPrefab, _standingsContainer);
        row.name = rowName;
        row.gameObject.SetActive(true);
        return row;
    }

    private void ClearRows()
    {
        for (int i = _standingsContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = _standingsContainer.GetChild(i);
            if (child == _standingRowPrefab.transform)
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }

    private static Dictionary<string, TeamData> BuildTeamLookup(List<TeamData> teams)
    {
        Dictionary<string, TeamData> lookup = new Dictionary<string, TeamData>();
        if (teams == null)
        {
            return lookup;
        }

        foreach (TeamData team in teams)
        {
            if (team != null && !string.IsNullOrEmpty(team.Id) && !lookup.ContainsKey(team.Id))
            {
                lookup.Add(team.Id, team);
            }
        }

        return lookup;
    }

    private static List<string> BuildGroups(
        List<TeamStandingData> standings,
        Dictionary<string, TeamData> teamsById,
        bool groupByConference)
    {
        List<string> groups = new List<string>();
        foreach (TeamStandingData standing in standings)
        {
            string group = GetGroupName(FindTeam(teamsById, standing.TeamId), groupByConference);
            if (!groups.Contains(group))
            {
                groups.Add(group);
            }
        }

        groups.Sort();
        return groups;
    }

    private static List<TeamStandingData> FilterGroup(
        List<TeamStandingData> standings,
        Dictionary<string, TeamData> teamsById,
        string group,
        bool groupByConference)
    {
        List<TeamStandingData> result = new List<TeamStandingData>();
        foreach (TeamStandingData standing in standings)
        {
            TeamData team = FindTeam(teamsById, standing.TeamId);
            if (GetGroupName(team, groupByConference) == group)
            {
                result.Add(standing);
            }
        }

        return result;
    }

    private static TeamData FindTeam(Dictionary<string, TeamData> teamsById, string teamId)
    {
        if (teamsById == null || string.IsNullOrEmpty(teamId))
        {
            return null;
        }

        return teamsById.TryGetValue(teamId, out TeamData team) ? team : null;
    }

    private static string GetGroupName(TeamData team, bool groupByConference)
    {
        if (team == null)
        {
            return "Other";
        }

        string value = groupByConference ? team.ConferenceName : team.DivisionName;
        return string.IsNullOrEmpty(value) ? "Other" : value;
    }

    private static int CompareStandings(TeamStandingData left, TeamStandingData right)
    {
        int pointsComparison = right.Points.CompareTo(left.Points);
        if (pointsComparison != 0)
        {
            return pointsComparison;
        }

        int winsComparison = right.Wins.CompareTo(left.Wins);
        if (winsComparison != 0)
        {
            return winsComparison;
        }

        int leftGoalDifference = left.GoalsFor - left.GoalsAgainst;
        int rightGoalDifference = right.GoalsFor - right.GoalsAgainst;
        int goalDifferenceComparison = rightGoalDifference.CompareTo(leftGoalDifference);
        if (goalDifferenceComparison != 0)
        {
            return goalDifferenceComparison;
        }

        return string.Compare(left.TeamName, right.TeamName, System.StringComparison.Ordinal);
    }
}
