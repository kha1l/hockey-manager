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

        _infoText.text = prospect.FirstName + " " + prospect.LastName
            + " | " + prospect.Position
            + " | " + prospect.Age
            + " | OVR " + prospect.Overall
            + " | POT " + prospect.Potential
            + " | R" + prospect.DraftRound
            + " | #" + prospect.DraftPickOverall;
    }
}
