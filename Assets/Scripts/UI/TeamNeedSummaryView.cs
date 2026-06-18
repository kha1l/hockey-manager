using UnityEngine;
using UnityEngine.UI;

public class TeamNeedSummaryView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(TeamTradeProfileData profile)
    {
        if (_infoText == null)
        {
            return;
        }

        if (profile == null || profile.Needs == null)
        {
            _infoText.text = "Team needs недоступны";
            return;
        }

        TeamNeedData needs = profile.Needs;
        _infoText.text = profile.TeamName
            + " | " + profile.Direction
            + " | Primary: " + needs.PrimaryNeed
            + " | Secondary: " + needs.SecondaryNeed
            + "\nTop6F " + needs.NeedTop6Forward
            + " | D " + needs.NeedDefenseman
            + " | G " + needs.NeedGoalie
            + " | Cap " + needs.NeedCapSpace
            + " | Roster " + needs.NeedRosterSpace
            + " | Buyer " + profile.BuyerScore
            + " | Seller " + profile.SellerScore;
    }
}
