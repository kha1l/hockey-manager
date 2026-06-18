using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class WaiverRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;
    [SerializeField] private Button _selectButton;

    private string _waiverId;
    private GameScreenController _screenController;

    public void Configure(Text infoText, Button selectButton)
    {
        _infoText = infoText;
        _selectButton = selectButton;
    }

    public void Initialize(WaiverPlayerData waiver, GameScreenController screenController)
    {
        _waiverId = waiver == null ? "" : waiver.WaiverId;
        _screenController = screenController;

        if (_infoText != null)
        {
            _infoText.text = BuildText(waiver);
        }

        if (_selectButton != null)
        {
            _selectButton.onClick.RemoveAllListeners();
            _selectButton.onClick.AddListener(OnSelectClicked);
        }
    }

    private void OnSelectClicked()
    {
        if (_screenController != null)
        {
            _screenController.SelectWaiver(_waiverId);
        }
    }

    private static string BuildText(WaiverPlayerData waiver)
    {
        if (waiver == null)
        {
            return "Waiver entry не найден";
        }

        string resolution = string.IsNullOrEmpty(waiver.Resolution) ? "" : " | " + waiver.Resolution;
        return waiver.PlayerName
            + " | " + waiver.Position
            + " | OVR " + waiver.Overall
            + " | Age " + waiver.Age
            + " | $" + FormatMoney(waiver.Salary)
            + " | " + waiver.OriginalTeamName
            + " | Days " + waiver.DaysRemaining
            + " | " + waiver.Status
            + resolution;
    }

    private static string FormatMoney(int value)
    {
        return value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
    }
}
