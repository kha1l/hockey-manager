using UnityEngine;
using UnityEngine.UI;

public class UserTeamHistoryRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(UserTeamSeasonHistoryData history)
    {
        if (_infoText == null)
        {
            return;
        }

        if (history == null)
        {
            _infoText.text = "User team history unavailable";
            return;
        }

        _infoText.text = history.SeasonStartYear + "-" + (history.SeasonEndYear % 100).ToString("D2")
            + " | " + history.TeamName
            + " | " + history.Wins + "-" + history.Losses + "-" + history.OvertimeLosses
            + " | " + history.Points + " pts"
            + " | " + history.PlayoffResult
            + " | Trust " + history.GmTrustAfterSeason
            + " | " + history.JobSecurityAfterSeason
            + (string.IsNullOrEmpty(history.OwnerEvaluationSummary) ? "" : " | " + history.OwnerEvaluationSummary);
    }
}
