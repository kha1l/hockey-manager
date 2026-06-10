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
}
