using UnityEngine;
using UnityEngine.UI;

public class DraftRightsRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(ProspectData prospect)
    {
        if (prospect == null)
        {
            _infoText.text = "Права недоступны";
            return;
        }

        int rank = ScoutingService.GetProspectRank(prospect, prospect.DraftPickOverall);
        ScoutingService.EnsureProspectScouting(prospect, rank);
        _infoText.text = prospect.FirstName + " " + prospect.LastName
            + " | #" + rank
            + " | " + prospect.ProjectedRound
            + " | " + prospect.ProspectArchetype
            + " | " + prospect.Position
            + " | " + prospect.Age
            + " | OVR " + prospect.Overall
            + " | POT " + prospect.Potential
            + " | " + prospect.RiskHint
            + " | " + prospect.DevelopmentTypeHint
            + " | " + prospect.CeilingHint
            + " | " + prospect.FloorHint
            + " | R" + prospect.DraftRound
            + " | #" + prospect.DraftPickOverall;
    }
}
