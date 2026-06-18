using UnityEngine;
using UnityEngine.UI;

public class TeamButtonView : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _logoImage;
    [SerializeField] private Text _cityText;
    [SerializeField] private Text _nameText;
    [SerializeField] private Text _abbreviationText;
    [SerializeField] private Text _divisionText;

    private TeamData _team;
    private TeamSelectController _controller;

    public void Configure(Button button, Text cityText, Text nameText, Text abbreviationText)
    {
        Configure(button, null, cityText, nameText, abbreviationText, null);
    }

    public void Configure(Button button, Image logoImage, Text cityText, Text nameText, Text abbreviationText, Text divisionText)
    {
        _button = button;
        _logoImage = logoImage;
        _cityText = cityText;
        _nameText = nameText;
        _abbreviationText = abbreviationText;
        _divisionText = divisionText;
    }

    public void Initialize(TeamData team, TeamSelectController controller)
    {
        _team = team;
        _controller = controller;
        TeamIdentityService.EnsureTeamIdentity(_team);

        if (_cityText != null)
        {
            _cityText.text = _team.City;
        }

        if (_nameText != null)
        {
            _nameText.text = TeamIdentityService.GetDisplayName(_team);
        }

        if (_abbreviationText != null)
        {
            _abbreviationText.text = TeamIdentityService.GetAbbreviation(_team);
        }

        if (_divisionText != null)
        {
            _divisionText.text = _team.ConferenceName + " / " + _team.DivisionName;
        }

        if (_logoImage != null)
        {
            _logoImage.sprite = TeamAssetService.LoadLogo(_team);
            _logoImage.color = _logoImage.sprite == null ? TeamIdentityService.GetPrimaryColor(_team) : Color.white;
            _logoImage.preserveAspect = true;
        }

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        _controller.SelectTeam(_team.Id);
    }
}
