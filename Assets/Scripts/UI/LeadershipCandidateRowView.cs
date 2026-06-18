using UnityEngine;
using UnityEngine.UI;

public class LeadershipCandidateRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;
    [SerializeField] private Button _button;

    private string _playerId;
    private GameScreenController _screenController;

    public void Configure(Text infoText, Button button)
    {
        _infoText = infoText;
        _button = button;
    }

    public void Initialize(LeadershipCandidateData candidate, GameScreenController screenController)
    {
        _playerId = candidate == null ? "" : candidate.PlayerId;
        _screenController = screenController;

        if (_infoText != null)
        {
            _infoText.text = FormatCandidate(candidate);
        }

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnClicked);
        }
    }

    private void OnClicked()
    {
        if (_screenController != null && !string.IsNullOrEmpty(_playerId))
        {
            _screenController.SelectLeadershipPlayer(_playerId);
        }
    }

    private static string FormatCandidate(LeadershipCandidateData candidate)
    {
        if (candidate == null)
        {
            return "Игрок недоступен";
        }

        string marker = candidate.CurrentCaptaincyRole == LeadershipConfig.RoleCaptain
            ? "C"
            : (candidate.CurrentCaptaincyRole == LeadershipConfig.RoleAlternate ? "A" : "-");

        return marker
            + " | " + candidate.PlayerName
            + " | " + candidate.Position
            + " | Age " + candidate.Age
            + " | OVR " + candidate.Overall
            + " | " + candidate.RosterStatus
            + " | MOR " + candidate.Morale
            + " | LD " + candidate.Leadership
            + " | PRO " + candidate.Professionalism
            + " | INF " + candidate.LockerRoomInfluence
            + " | Score " + candidate.CaptaincyScore
            + " | " + (candidate.IsEligible ? "eligible" : "not eligible");
    }
}
