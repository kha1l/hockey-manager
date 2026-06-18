using UnityEngine;
using UnityEngine.UI;

public class MoraleEventRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(MoraleEventData moraleEvent)
    {
        if (_infoText == null)
        {
            return;
        }

        if (moraleEvent == null)
        {
            _infoText.text = "Событие морали недоступно";
            return;
        }

        _infoText.text = moraleEvent.PlayerName
            + " | " + moraleEvent.EventType
            + " | " + moraleEvent.MoraleBefore + " -> " + moraleEvent.MoraleAfter
            + " | Delta " + moraleEvent.Delta
            + " | " + moraleEvent.Reason
            + " | " + moraleEvent.CreatedAtUtc;
    }
}
