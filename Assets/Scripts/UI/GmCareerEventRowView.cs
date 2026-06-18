using UnityEngine;
using UnityEngine.UI;

public class GmCareerEventRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(GmCareerEventData careerEvent)
    {
        if (_infoText == null)
        {
            return;
        }

        if (careerEvent == null)
        {
            _infoText.text = "Career event unavailable";
            return;
        }

        _infoText.text = careerEvent.SeasonStartYear + "-" + careerEvent.SeasonEndYear
            + " | " + careerEvent.EventType
            + " | " + careerEvent.Title
            + "\n" + careerEvent.Summary
            + " | Security " + careerEvent.JobSecurityBefore + " -> " + careerEvent.JobSecurityAfter
            + " | Trust " + careerEvent.TrustBefore + " -> " + careerEvent.TrustAfter;
    }
}
