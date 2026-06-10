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
        PlayerFatigueService.EnsureFatigueFields(player);
        InjuryService.EnsureInjuryFields(player);
        PlayerRoleService.EnsureRole(player);
        _nameText.text = player.FirstName + " " + player.LastName + (player.IsEntryLevelContract ? " (ELC)" : "") + (player.IsInjured ? " (INJ)" : "");
        _positionText.text = player.Position;
        _ageText.text = player.Age.ToString();
        _overallText.text = "OVR " + player.Overall
            + " EFF " + PlayerFatigueService.GetEffectiveOverall(player)
            + " " + FormatDevelopment(player.LastDevelopmentDelta)
            + " | " + player.PlayerRole
            + " | TOI " + IceTimeConfig.FormatSeconds(player.EstimatedTimeOnIceSeconds);
        string activeStatus = team == null
            ? ""
            : (LineupService.IsPlayerActive(team, player.Id) ? " Active" : " Scratch");
        string fatigueDelta = player.LastGameFatigueChange == 0
            ? ""
            : " (" + FormatSigned(player.LastGameFatigueChange) + ")";
        _potentialText.text = "POT " + player.Potential
            + " | COND " + player.Condition
            + " | FAT " + player.Fatigue + fatigueDelta
            + " | " + player.UsageCategory
            + " | ATOI " + IceTimeConfig.FormatSeconds(player.AverageTimeOnIceSeconds)
            + activeStatus
            + (player.IsInjured ? " | INJ " + player.InjuryDaysRemaining + " дн." : "");
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

    private static string FormatSigned(int value)
    {
        return value > 0 ? "+" + value : value.ToString();
    }
}
