using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeadershipController : MonoBehaviour
{
    [SerializeField] private Text _summaryText;
    [SerializeField] private Text _selectedPlayerText;
    [SerializeField] private Transform _candidatesContainer;
    [SerializeField] private LeadershipCandidateRowView _candidateRowPrefab;
    [SerializeField] private GameScreenController _screenController;

    public void Configure(
        Text summaryText,
        Text selectedPlayerText,
        Transform candidatesContainer,
        LeadershipCandidateRowView candidateRowPrefab,
        GameScreenController screenController)
    {
        _summaryText = summaryText;
        _selectedPlayerText = selectedPlayerText;
        _candidatesContainer = candidatesContainer;
        _candidateRowPrefab = candidateRowPrefab;
        _screenController = screenController;
    }

    public void ShowLeadership(GameState state, string selectedPlayerId)
    {
        if (_summaryText == null || _selectedPlayerText == null || _candidatesContainer == null || _candidateRowPrefab == null)
        {
            Debug.LogError("LeadershipController: UI references are not configured.");
            return;
        }

        GameSession.EnsureLeadership();
        TeamLeadershipData leadership = GameSession.GetCurrentTeamLeadership();
        List<LeadershipCandidateData> candidates = GameSession.GetCurrentTeamLeadershipCandidates();

        _summaryText.text = FormatSummary(leadership);
        _selectedPlayerText.text = FormatSelected(candidates, selectedPlayerId);
        RenderCandidates(candidates);
    }

    private void RenderCandidates(List<LeadershipCandidateData> candidates)
    {
        ClearRows(_candidatesContainer, _candidateRowPrefab.transform);
        foreach (LeadershipCandidateData candidate in candidates)
        {
            LeadershipCandidateRowView row = Instantiate(_candidateRowPrefab, _candidatesContainer);
            row.gameObject.SetActive(true);
            row.Initialize(candidate, _screenController);
        }

        _candidateRowPrefab.gameObject.SetActive(false);
    }

    private static string FormatSummary(TeamLeadershipData leadership)
    {
        if (leadership == null)
        {
            return "Leadership data недоступны";
        }

        string captain = string.IsNullOrEmpty(leadership.CaptainName)
            ? "No captain assigned"
            : leadership.CaptainName;
        string alternate1 = string.IsNullOrEmpty(leadership.Alternate1Name) ? "none" : leadership.Alternate1Name;
        string alternate2 = string.IsNullOrEmpty(leadership.Alternate2Name) ? "none" : leadership.Alternate2Name;

        return "Captain: " + captain
            + "\nAlternate Captains: " + alternate1 + " / " + alternate2
            + "\nLeadership: " + leadership.LeadershipScore + " " + leadership.LeadershipLabel
            + " | LockerRoomImpact " + leadership.LockerRoomImpact
            + "\nMorale impact: " + FormatSigned(leadership.MoraleImpact)
            + " | Chemistry impact: " + FormatSigned(leadership.ChemistryImpact)
            + "\n" + leadership.LeadershipSummary;
    }

    private static string FormatSelected(List<LeadershipCandidateData> candidates, string selectedPlayerId)
    {
        LeadershipCandidateData selected = FindCandidate(candidates, selectedPlayerId);
        if (selected == null)
        {
            return "Выберите игрока";
        }

        return selected.PlayerName
            + " | " + selected.Position
            + " | Age " + selected.Age
            + " | OVR " + selected.Overall
            + "\nRoster: " + selected.RosterStatus
            + " | Morale " + selected.Morale
            + " | WantsTrade " + (selected.WantsTrade ? "yes" : "no")
            + "\nLeadership " + selected.Leadership
            + " | Professionalism " + selected.Professionalism
            + " | Influence " + selected.LockerRoomInfluence
            + "\nCaptaincyScore " + selected.CaptaincyScore
            + " | Role " + selected.CurrentCaptaincyRole
            + " | Eligible " + (selected.IsEligible ? "yes" : "no")
            + "\n" + selected.CandidateSummary;
    }

    private static LeadershipCandidateData FindCandidate(List<LeadershipCandidateData> candidates, string playerId)
    {
        if (candidates == null || string.IsNullOrEmpty(playerId))
        {
            return null;
        }

        foreach (LeadershipCandidateData candidate in candidates)
        {
            if (candidate != null && candidate.PlayerId == playerId)
            {
                return candidate;
            }
        }

        return null;
    }

    private static void ClearRows(Transform container, Transform template)
    {
        if (container == null)
        {
            return;
        }

        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Transform child = container.GetChild(i);
            if (template != null && child == template)
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }

    private static string FormatSigned(int value)
    {
        return value > 0 ? "+" + value : value.ToString();
    }
}
