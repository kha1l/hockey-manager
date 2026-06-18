using UnityEngine;
using UnityEngine.UI;

public class PlayerRowView : MonoBehaviour
{
    [SerializeField] private Text _nameText;
    [SerializeField] private Text _positionText;
    [SerializeField] private Text _ageText;
    [SerializeField] private Text _overallText;
    [SerializeField] private Text _potentialText;

    public void Configure(Text nameText, Text positionText, Text ageText, Text overallText, Text potentialText)
    {
        _nameText = nameText;
        _positionText = positionText;
        _ageText = ageText;
        _overallText = overallText;
        _potentialText = potentialText;
    }

    public void Initialize(PlayerData player)
    {
        Initialize(player, null);
    }

    public void Initialize(PlayerData player, TeamData team)
    {
        if (player == null)
        {
            SetText(_nameText, "Игрок не найден");
            SetText(_positionText, "");
            SetText(_ageText, "");
            SetText(_overallText, "");
            SetText(_potentialText, "");
            return;
        }

        _nameText.text = PlayerDisplayFormatter.FormatPlayerName(player);
        _positionText.text = player.Position;
        _ageText.text = player.Age.ToString();
        _overallText.text = PlayerDisplayFormatter.FormatPlayerMainLine(player)
            + "\n" + PlayerDisplayFormatter.FormatPlayerSubLine(player);
        string activeStatus = team == null
            ? ""
            : (LineupService.IsPlayerActive(team, player.Id) ? " Active" : " Scratch");
        _potentialText.text = "EFF " + PlayerFatigueService.GetEffectiveOverall(player)
            + " | " + player.UsageCategory
            + " | ATOI " + IceTimeConfig.FormatSeconds(player.AverageTimeOnIceSeconds)
            + activeStatus;
    }

    private static void SetText(Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }
}
