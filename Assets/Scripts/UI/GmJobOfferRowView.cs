using UnityEngine;
using UnityEngine.UI;

public class GmJobOfferRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;
    [SerializeField] private Button _selectButton;

    private GmJobOfferData _offer;
    private GameScreenController _screenController;

    public void Configure(Text infoText, Button selectButton)
    {
        _infoText = infoText;
        _selectButton = selectButton;
    }

    public void Initialize(GmJobOfferData offer, GameScreenController screenController)
    {
        _offer = offer;
        _screenController = screenController;

        if (_infoText != null)
        {
            if (offer == null)
            {
                _infoText.text = "Job offer unavailable";
            }
            else
            {
                _infoText.text = offer.TeamName
                    + " | " + offer.TeamDirection
                    + " | Pts " + offer.LastSeasonPoints
                    + " | OVR " + offer.TeamOverall
                    + "\n" + offer.OfferReason
                    + " | Security " + offer.JobSecurityStartingValue;
            }
        }

        if (_selectButton != null)
        {
            _selectButton.onClick.RemoveAllListeners();
            _selectButton.onClick.AddListener(SelectOffer);
        }
    }

    private void SelectOffer()
    {
        if (_screenController != null && _offer != null)
        {
            _screenController.SelectGmJobOffer(_offer.OfferId);
        }
    }
}
