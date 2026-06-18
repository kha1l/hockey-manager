using UnityEngine;
using UnityEngine.UI;

public class StaffController : MonoBehaviour
{
    [SerializeField] private Text _summaryText;
    [SerializeField] private Text _effectsText;
    [SerializeField] private Transform _staffContainer;
    [SerializeField] private StaffMemberRowView _rowPrefab;

    public void Configure(
        Text summaryText,
        Text effectsText,
        Transform staffContainer,
        StaffMemberRowView rowPrefab)
    {
        _summaryText = summaryText;
        _effectsText = effectsText;
        _staffContainer = staffContainer;
        _rowPrefab = rowPrefab;
    }

    public void ShowStaff(GameState state)
    {
        if (_summaryText == null || _effectsText == null || _staffContainer == null || _rowPrefab == null)
        {
            Debug.LogError("StaffController: UI references are not configured.");
            return;
        }

        TeamData team = FindTeam(state);
        CoachingStaffService.EnsureStaffForTeam(team);
        RenderSummary(team);
        RenderRows(team);
    }

    private void RenderSummary(TeamData team)
    {
        if (team == null || team.Staff == null)
        {
            _summaryText.text = "Команда не выбрана";
            _effectsText.text = "";
            return;
        }

        TeamStaffData staff = team.Staff;
        StaffData headCoach = staff.HeadCoach;
        StaffEffectSummaryData effects = CoachingStaffService.BuildStaffEffectSummary(team);
        _summaryText.text = "Команда: " + TeamIdentityService.GetDisplayName(team)
            + "\nHead Coach: " + (headCoach == null ? "none" : headCoach.FullName)
            + " | Style: " + (headCoach == null ? StaffConfig.StyleBalanced : headCoach.CoachingStyle)
            + " | Staff overall: " + staff.StaffOverall + " " + StaffConfig.GetStaffQualityLabel(staff.StaffOverall)
            + "\n" + staff.StaffSummary;

        _effectsText.text = "Staff effects"
            + "\nTeam rating " + FormatSigned(effects.TeamRatingModifier)
            + " | Off " + FormatSigned(effects.OffenseModifier)
            + " | Def " + FormatSigned(effects.DefenseModifier)
            + " | PP " + FormatSigned(effects.PowerPlayModifier)
            + " | PK " + FormatSigned(effects.PenaltyKillModifier)
            + "\nDev " + FormatSigned(effects.DevelopmentModifier)
            + " | Goalie Dev " + FormatSigned(effects.GoalieDevelopmentModifier)
            + " | Morale " + FormatSigned(effects.MoraleModifier)
            + " | Chemistry " + FormatSigned(effects.ChemistryModifier)
            + "\nTactical fit " + FormatSigned(CoachingStaffService.GetTacticalFitModifier(team))
            + " | Discipline " + FormatSigned(effects.DisciplineModifier);
    }

    private void RenderRows(TeamData team)
    {
        ClearRows();
        _rowPrefab.gameObject.SetActive(false);

        if (team == null || team.Staff == null)
        {
            return;
        }

        AddRow(team.Staff.HeadCoach);
        AddRow(team.Staff.AssistantCoach);
        AddRow(team.Staff.DevelopmentCoach);
        AddRow(team.Staff.GoalieCoach);
    }

    private void AddRow(StaffData staff)
    {
        StaffMemberRowView row = Instantiate(_rowPrefab, _staffContainer);
        row.gameObject.SetActive(true);
        row.Initialize(staff);
    }

    private void ClearRows()
    {
        Transform template = _rowPrefab.transform;
        for (int i = _staffContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = _staffContainer.GetChild(i);
            if (child == template)
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }

    private static TeamData FindTeam(GameState state)
    {
        if (state == null || state.Teams == null || string.IsNullOrEmpty(state.SelectedTeamId))
        {
            return null;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team != null && team.Id == state.SelectedTeamId)
            {
                return team;
            }
        }

        return null;
    }

    private static string FormatSigned(int value)
    {
        return value > 0 ? "+" + value : value.ToString();
    }
}
