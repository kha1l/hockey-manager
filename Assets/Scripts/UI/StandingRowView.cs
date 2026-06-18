using UnityEngine;
using UnityEngine.UI;

public class StandingRowView : MonoBehaviour
{
    [SerializeField] private Text _placeText;
    [SerializeField] private Text _teamNameText;
    [SerializeField] private Text _gamesPlayedText;
    [SerializeField] private Text _winsText;
    [SerializeField] private Text _lossesText;
    [SerializeField] private Text _overtimeLossesText;
    [SerializeField] private Text _pointsText;
    [SerializeField] private Text _goalsForText;
    [SerializeField] private Text _goalsAgainstText;

    public void Configure(
        Text placeText,
        Text teamNameText,
        Text gamesPlayedText,
        Text winsText,
        Text lossesText,
        Text overtimeLossesText,
        Text pointsText,
        Text goalsForText,
        Text goalsAgainstText)
    {
        _placeText = placeText;
        _teamNameText = teamNameText;
        _gamesPlayedText = gamesPlayedText;
        _winsText = winsText;
        _lossesText = lossesText;
        _overtimeLossesText = overtimeLossesText;
        _pointsText = pointsText;
        _goalsForText = goalsForText;
        _goalsAgainstText = goalsAgainstText;
    }

    public void Initialize(int place, TeamStandingData standing)
    {
        _placeText.text = place.ToString();
        _teamNameText.text = standing.TeamName;
        _gamesPlayedText.text = standing.GamesPlayed.ToString();
        _winsText.text = standing.Wins.ToString();
        _lossesText.text = standing.Losses.ToString();
        _overtimeLossesText.text = standing.OvertimeLosses.ToString();
        _pointsText.text = standing.Points.ToString();
        _goalsForText.text = standing.GoalsFor.ToString();
        _goalsAgainstText.text = standing.GoalsAgainst.ToString();
    }

    public void InitializeDetailed(int place, TeamStandingData standing, TeamData team)
    {
        Initialize(place, standing);

        int goalDifference = standing.GoalsFor - standing.GoalsAgainst;
        string fullTeamName = GetFullTeamName(standing, team);
        string groupText = team == null ? "" : " | " + SafeText(team.ConferenceName) + " / " + SafeText(team.DivisionName);
        string percentText = standing.GamesPlayed <= 0
            ? ".000"
            : (standing.Points / (double)(standing.GamesPlayed * 2)).ToString(".000");
        _teamNameText.text = fullTeamName
            + groupText
            + " | P% " + percentText
            + " | +/- " + FormatSigned(goalDifference);
    }

    public void InitializeMessage(string message)
    {
        _placeText.text = "";
        _teamNameText.text = message;
        _gamesPlayedText.text = "";
        _winsText.text = "";
        _lossesText.text = "";
        _overtimeLossesText.text = "";
        _pointsText.text = "";
        _goalsForText.text = "";
        _goalsAgainstText.text = "";
    }

    private static string GetFullTeamName(TeamStandingData standing, TeamData team)
    {
        if (team != null)
        {
            string city = SafeText(team.City);
            string name = SafeText(team.Name);
            if (!string.IsNullOrEmpty(city) && !string.IsNullOrEmpty(name))
            {
                return city + " " + name;
            }

            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }
        }

        return standing == null ? "" : SafeText(standing.TeamName);
    }

    private static string SafeText(string value)
    {
        return string.IsNullOrEmpty(value) ? "" : value;
    }

    private static string FormatSigned(int value)
    {
        return value > 0 ? "+" + value : value.ToString();
    }
}
