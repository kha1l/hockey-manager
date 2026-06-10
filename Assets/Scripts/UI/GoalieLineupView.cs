using UnityEngine;
using UnityEngine.UI;

public class GoalieLineupView : MonoBehaviour
{
    [SerializeField] private Text _infoText;
    [SerializeField] private Button _starterButton;
    [SerializeField] private Button _backupButton;
    [SerializeField] private GameScreenController _screenController;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Configure(Text infoText, Button starterButton, Button backupButton, GameScreenController screenController)
    {
        _infoText = infoText;
        _starterButton = starterButton;
        _backupButton = backupButton;
        _screenController = screenController;
    }

    public void Initialize(GoalieLineupData goalies, TeamData team)
    {
        if (_infoText == null)
        {
            return;
        }

        PlayerData starter = goalies == null ? null : FindPlayer(team, goalies.StarterGoaliePlayerId);
        PlayerData backup = goalies == null ? null : FindPlayer(team, goalies.BackupGoaliePlayerId);
        _infoText.text = "Стартовый: " + FormatPlayer(starter)
            + "\nЗапасной: " + FormatPlayer(backup);

        ConfigureButton(_starterButton, "Starter");
        ConfigureButton(_backupButton, "Backup");
    }

    private static string FormatPlayer(PlayerData player)
    {
        if (player == null)
        {
            return "пусто";
        }

        PlayerFatigueService.EnsureFatigueFields(player);
        InjuryService.EnsureInjuryFields(player);
        PlayerRoleService.EnsureRole(player);
        return player.FirstName + " " + player.LastName
            + " (" + player.Position
            + " OVR " + player.Overall
            + " EFF " + PlayerFatigueService.GetEffectiveOverall(player)
            + " " + player.PlayerRole
            + " TOI " + IceTimeConfig.FormatSeconds(player.EstimatedTimeOnIceSeconds)
            + " COND " + player.Condition
            + " FAT " + player.Fatigue
            + (player.IsInjured ? " INJ " + player.InjuryDaysRemaining + "д" : "") + ")";
    }

    private static PlayerData FindPlayer(TeamData team, string playerId)
    {
        if (team == null || string.IsNullOrEmpty(playerId))
        {
            return null;
        }

        team.EnsurePlayers();
        foreach (PlayerData player in team.Players)
        {
            if (player != null && player.Id == playerId)
            {
                return player;
            }
        }

        return null;
    }

    private void ConfigureButton(Button button, string slotPosition)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            if (_screenController != null)
            {
                _screenController.SelectLineupSlot("Goalie", 1, slotPosition);
            }
        });
    }
}
