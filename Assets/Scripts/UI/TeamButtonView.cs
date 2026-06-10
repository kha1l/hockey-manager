using UnityEngine;
using UnityEngine.UI;

public class TeamButtonView : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Text _cityText;
    [SerializeField] private Text _nameText;
    [SerializeField] private Text _abbreviationText;

    private TeamData _team;
    private TeamSelectController _controller;

    public void Configure(Button button, Text cityText, Text nameText, Text abbreviationText)
    {
        _button = button;
        _cityText = cityText;
        _nameText = nameText;
        _abbreviationText = abbreviationText;
    }

    public void Initialize(TeamData team, TeamSelectController controller)
    {
        _team = team;
        _controller = controller;

        _cityText.text = _team.City;
        _nameText.text = _team.Name;
        _abbreviationText.text = _team.Abbreviation;

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        _controller.SelectTeam(_team.Id);
    }
}
