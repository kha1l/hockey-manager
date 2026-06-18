using UnityEngine;
using UnityEngine.UI;

public class AwardWinnerRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(AwardWinnerData award)
    {
        if (_infoText == null)
        {
            return;
        }

        if (award == null)
        {
            _infoText.text = "Award unavailable";
            return;
        }

        _infoText.text = award.SeasonStartYear + "-" + (award.SeasonEndYear % 100).ToString("D2")
            + " | " + award.AwardName
            + " | " + award.PlayerName
            + " | " + award.TeamName
            + "\n" + award.Position
            + " | " + MobileUiConfig.FormatShortStatus(award.Reason);
    }
}
