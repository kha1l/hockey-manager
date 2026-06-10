using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class ContractRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;
    [SerializeField] private Button _extendButton;

    private string _playerId;
    private ContractsController _controller;

    public void Configure(Text infoText, Button extendButton)
    {
        _infoText = infoText;
        _extendButton = extendButton;
    }

    public void Initialize(PlayerData player, ContractsController controller)
    {
        _playerId = player.Id;
        _controller = controller;
        InjuryService.EnsureInjuryFields(player);
        string contractLabel = player.ContractStatus
            + (player.IsEntryLevelContract ? " | ELC" : "")
            + (player.IsInjured ? " | INJ " + player.InjuryDaysRemaining + " дн." : "");
        _infoText.text = player.FirstName + " " + player.LastName
            + " | " + player.Position
            + " | " + player.Age
            + " лет | OVR " + player.Overall + " " + FormatDevelopment(player.LastDevelopmentDelta)
            + " | $" + FormatMoney(player.Salary)
            + " | " + player.ContractYearsRemaining + " г."
            + " | " + contractLabel;

        _extendButton.onClick.RemoveAllListeners();
        _extendButton.onClick.AddListener(OnExtendClicked);
    }

    private void OnExtendClicked()
    {
        if (_controller != null)
        {
            _controller.ExtendContract(_playerId);
        }
    }

    private static string FormatMoney(int value)
    {
        return value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
    }

    private static string FormatDevelopment(int value)
    {
        if (value > 0)
        {
            return "DEV +" + value;
        }

        if (value < 0)
        {
            return "DEV " + value;
        }

        return "DEV 0";
    }
}
