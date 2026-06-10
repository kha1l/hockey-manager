using UnityEngine;
using UnityEngine.UI;

public class TacticsController : MonoBehaviour
{
    [SerializeField] private Text _presetText;
    [SerializeField] private Text _parametersText;
    [SerializeField] private Text _ratingsText;
    [SerializeField] private Transform _powerPlayContainer;
    [SerializeField] private Transform _penaltyKillContainer;
    [SerializeField] private PowerPlayUnitRowView _powerPlayRowPrefab;
    [SerializeField] private PenaltyKillUnitRowView _penaltyKillRowPrefab;

    public void Configure(
        Text presetText,
        Text parametersText,
        Text ratingsText,
        Transform powerPlayContainer,
        Transform penaltyKillContainer,
        PowerPlayUnitRowView powerPlayRowPrefab,
        PenaltyKillUnitRowView penaltyKillRowPrefab)
    {
        _presetText = presetText;
        _parametersText = parametersText;
        _ratingsText = ratingsText;
        _powerPlayContainer = powerPlayContainer;
        _penaltyKillContainer = penaltyKillContainer;
        _powerPlayRowPrefab = powerPlayRowPrefab;
        _penaltyKillRowPrefab = penaltyKillRowPrefab;
    }

    public void ShowTactics(TeamData team)
    {
        if (_presetText == null || _parametersText == null || _ratingsText == null)
        {
            Debug.LogError("TacticsController: UI references are not configured.");
            return;
        }

        SpecialTeamsService.EnsureSpecialTeams(team);
        TacticsService.EnsureTactics(team);
        RenderTexts(team);
        RenderPowerPlay(team);
        RenderPenaltyKill(team);
    }

    private void RenderTexts(TeamData team)
    {
        if (team == null)
        {
            _presetText.text = "Команда не выбрана";
            _parametersText.text = "";
            _ratingsText.text = "";
            return;
        }

        TeamTacticsData tactics = team.Tactics;
        _presetText.text = team.City + " " + team.Name
            + "\nPreset: " + tactics.PresetName;
        _parametersText.text = "OffensiveFocus: " + tactics.OffensiveFocus
            + " | DefensiveFocus: " + tactics.DefensiveFocus
            + "\nAggressiveness: " + tactics.Aggressiveness
            + " | Tempo: " + tactics.Tempo
            + "\nShootingFrequency: " + tactics.ShootingFrequency
            + " | RiskLevel: " + tactics.RiskLevel;

        bool isValid = SpecialTeamsService.ValidateSpecialTeams(team, out string message);
        _ratingsText.text = "PP rating: " + SpecialTeamsService.CalculatePowerPlayRating(team)
            + " | PK rating: " + SpecialTeamsService.CalculatePenaltyKillRating(team)
            + "\nСпецбригады: " + (isValid ? "валидны" : "требуют исправления")
            + "\n" + message;

        if (!isValid)
        {
            _ratingsText.text += "\nНажмите Автоспецбригады, чтобы исправить состав";
        }
    }

    private void RenderPowerPlay(TeamData team)
    {
        ClearRows(_powerPlayContainer, _powerPlayRowPrefab == null ? null : _powerPlayRowPrefab.transform);
        if (team == null || team.SpecialTeams == null || _powerPlayRowPrefab == null)
        {
            return;
        }

        foreach (PowerPlayUnitData unit in team.SpecialTeams.PowerPlayUnits)
        {
            PowerPlayUnitRowView row = Instantiate(_powerPlayRowPrefab, _powerPlayContainer);
            row.gameObject.SetActive(true);
            row.Initialize(unit, team);
        }

        _powerPlayRowPrefab.gameObject.SetActive(false);
    }

    private void RenderPenaltyKill(TeamData team)
    {
        ClearRows(_penaltyKillContainer, _penaltyKillRowPrefab == null ? null : _penaltyKillRowPrefab.transform);
        if (team == null || team.SpecialTeams == null || _penaltyKillRowPrefab == null)
        {
            return;
        }

        foreach (PenaltyKillUnitData unit in team.SpecialTeams.PenaltyKillUnits)
        {
            PenaltyKillUnitRowView row = Instantiate(_penaltyKillRowPrefab, _penaltyKillContainer);
            row.gameObject.SetActive(true);
            row.Initialize(unit, team);
        }

        _penaltyKillRowPrefab.gameObject.SetActive(false);
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
}
