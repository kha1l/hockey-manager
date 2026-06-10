using UnityEngine;
using UnityEngine.UI;

public class ProspectRightsRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;
    [SerializeField] private Button _button;

    private string _prospectId;
    private GameScreenController _screenController;

    public void Configure(Text infoText, Button button)
    {
        _infoText = infoText;
        _button = button;
    }

    public void Initialize(ProspectData prospect, GameScreenController screenController)
    {
        _prospectId = prospect.Id;
        _screenController = screenController;
        int elcYears = EntryLevelContractConfig.GetContractYearsByAge(prospect.Age);
        string elcText = elcYears > 0 ? elcYears + " г. ELC" : "ELC недоступен";

        _infoText.text = prospect.FirstName + " " + prospect.LastName
            + " | " + prospect.Position
            + " | " + prospect.Age
            + " | OVR " + prospect.Overall
            + " | POT " + prospect.Potential
            + " | " + FormatDevelopment(prospect.LastDevelopmentDelta)
            + " | R" + prospect.DraftRound
            + " | #" + prospect.DraftPickOverall
            + " | " + elcText;

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        if (_screenController != null)
        {
            _screenController.SelectProspectRights(_prospectId);
        }
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
