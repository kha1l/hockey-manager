using UnityEngine;
using UnityEngine.UI;

public class ProspectRowView : MonoBehaviour
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
        _prospectId = prospect == null ? "" : prospect.Id;
        _screenController = screenController;
        _infoText.text = PlayerDisplayFormatter.FormatProspect(prospect);

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        if (_screenController != null)
        {
            _screenController.SelectProspect(_prospectId);
        }
    }

}
