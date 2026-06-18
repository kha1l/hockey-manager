using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LineupController : MonoBehaviour
{
    [SerializeField] private Text _statusText;
    [SerializeField] private Text _ratingsText;
    [SerializeField] private Text _selectedSlotText;
    [SerializeField] private Text _selectedPlayerText;
    [SerializeField] private Transform _lineupSlotsContainer;
    [SerializeField] private Transform _eligiblePlayersContainer;
    [SerializeField] private Transform _scratchPlayersContainer;
    [SerializeField] private LineupSlotRowView _lineupSlotRowPrefab;
    [SerializeField] private LineupEligiblePlayerRowView _eligiblePlayerRowPrefab;
    [SerializeField] private ScratchPlayerRowView _scratchPlayerRowPrefab;
    [SerializeField] private GameScreenController _screenController;

    public void Configure(
        Text statusText,
        Text ratingsText,
        Text selectedSlotText,
        Text selectedPlayerText,
        Transform lineupSlotsContainer,
        Transform eligiblePlayersContainer,
        Transform scratchPlayersContainer,
        LineupSlotRowView lineupSlotRowPrefab,
        LineupEligiblePlayerRowView eligiblePlayerRowPrefab,
        ScratchPlayerRowView scratchPlayerRowPrefab,
        GameScreenController screenController)
    {
        _statusText = statusText;
        _ratingsText = ratingsText;
        _selectedSlotText = selectedSlotText;
        _selectedPlayerText = selectedPlayerText;
        _lineupSlotsContainer = lineupSlotsContainer;
        _eligiblePlayersContainer = eligiblePlayersContainer;
        _scratchPlayersContainer = scratchPlayersContainer;
        _lineupSlotRowPrefab = lineupSlotRowPrefab;
        _eligiblePlayerRowPrefab = eligiblePlayerRowPrefab;
        _scratchPlayerRowPrefab = scratchPlayerRowPrefab;
        _screenController = screenController;
    }

    public void ShowLineup(
        TeamData team,
        string selectedSlotType,
        int selectedLineOrPairNumber,
        string selectedSlotPosition,
        string selectedPlayerId)
    {
        if (_statusText == null || _ratingsText == null)
        {
            Debug.LogError("LineupController: UI references are not configured.");
            return;
        }

        LineupService.EnsureLineup(team);
        ChemistryService.EnsureChemistryForTeam(team);
        RenderStatus(team);
        RenderRatings(team);
        RenderSelection(team, selectedSlotType, selectedLineOrPairNumber, selectedSlotPosition, selectedPlayerId);
        RenderLineupSlots(team);
        RenderEligiblePlayers(team, selectedSlotType, selectedSlotPosition, selectedPlayerId);
        RenderScratches(team);
    }

    public void ShowLineup(TeamData team)
    {
        ShowLineup(team, "", 0, "", "");
    }

    private void RenderStatus(TeamData team)
    {
        if (team == null)
        {
            _statusText.text = "Команда не выбрана";
            return;
        }

        bool isValid = LineupService.ValidateLineup(team, out string message);
        string manualText = team.Lineup != null && team.Lineup.IsManual ? "да" : "нет";
        string manualDate = team.Lineup != null && team.Lineup.IsManual && !string.IsNullOrEmpty(team.Lineup.LastManualUpdateUtc)
            ? "\nПоследнее ручное изменение: " + team.Lineup.LastManualUpdateUtc
            : "";

        _statusText.text = TeamIdentityService.GetDisplayName(team)
            + "\nСтатус: " + (isValid ? "валиден" : "требует исправления")
            + " | Ручной состав: " + manualText
            + "\n" + message
            + manualDate;

        if (!isValid)
        {
            _statusText.text += "\nИсправьте вручную или нажмите Автосостав";
        }
    }

    private void RenderRatings(TeamData team)
    {
        if (team == null)
        {
            _ratingsText.text = "Рейтинги недоступны";
            return;
        }

        PlayerFatigueService.EnsureFatigueForTeam(team);
        InjuryService.EnsureInjuryFieldsForTeam(team);
        TeamChemistryData chemistry = team.Chemistry ?? ChemistryService.CalculateTeamChemistry(team);
        int chemistryModifier = ChemistryConfig.GetTeamRatingModifier(chemistry.TeamChemistryScore);
        _ratingsText.text = "Offense: " + TeamRatingCalculator.CalculateOffenseRating(team)
            + " | Defense: " + TeamRatingCalculator.CalculateDefenseRating(team)
            + " | Goalie: " + TeamRatingCalculator.CalculateGoalieRating(team)
            + " | Total: " + TeamRatingCalculator.CalculateLineupOverall(team)
            + "\nEFF Offense: " + TeamRatingCalculator.CalculateEffectiveOffenseRating(team)
            + " | EFF Defense: " + TeamRatingCalculator.CalculateEffectiveDefenseRating(team)
            + " | EFF Goalie: " + TeamRatingCalculator.CalculateEffectiveGoalieRating(team)
            + " | EFF Total: " + TeamRatingCalculator.CalculateEffectiveLineupOverall(team)
            + "\nTeam Chemistry: " + chemistry.TeamChemistryScore + " " + chemistry.TeamChemistryLabel
            + " | Mod " + FormatSigned(chemistryModifier)
            + " | Best: " + chemistry.BestUnitName + " " + chemistry.BestUnitScore
            + " | Worst: " + chemistry.WorstUnitName + " " + chemistry.WorstUnitScore;
    }

    private static string FormatSigned(int value)
    {
        return value > 0 ? "+" + value : value.ToString();
    }

    private void RenderSelection(
        TeamData team,
        string selectedSlotType,
        int selectedLineOrPairNumber,
        string selectedSlotPosition,
        string selectedPlayerId)
    {
        if (_selectedSlotText != null)
        {
            _selectedSlotText.text = string.IsNullOrEmpty(selectedSlotType)
                ? "Выбранный слот: выберите слот в линиях"
                : "Выбранный слот: " + selectedSlotType + " " + selectedLineOrPairNumber + " " + selectedSlotPosition;
        }

        if (_selectedPlayerText == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(selectedSlotType))
        {
            _selectedPlayerText.text = "Выбранный игрок: сначала выберите слот";
            return;
        }

        if (string.IsNullOrEmpty(selectedPlayerId))
        {
            _selectedPlayerText.text = "Выбранный игрок: выберите игрока для назначения";
            return;
        }

        PlayerData player = FindPlayer(team, selectedPlayerId);
        if (player == null)
        {
            _selectedPlayerText.text = "Выбранный игрок: не найден";
            return;
        }

        InjuryService.EnsureInjuryFields(player);
        PlayerFatigueService.EnsureFatigueFields(player);
        _selectedPlayerText.text = "Выбранный игрок: " + player.FirstName + " " + player.LastName
            + " | " + player.Position
            + " | OVR " + player.Overall
            + " | EFF " + PlayerFatigueService.GetEffectiveOverall(player)
            + " | COND " + player.Condition
            + " | FAT " + player.Fatigue
            + (player.IsInjured ? "\nТравмированного игрока нельзя назначить" : "");
    }

    private void RenderLineupSlots(TeamData team)
    {
        ClearRows(_lineupSlotsContainer, _lineupSlotRowPrefab == null ? null : _lineupSlotRowPrefab.transform);
        if (team == null || _lineupSlotRowPrefab == null)
        {
            return;
        }

        List<LineupSlotData> slots = LineupService.GetLineupSlots(team);
        foreach (LineupSlotData slot in slots)
        {
            LineupSlotRowView row = Instantiate(_lineupSlotRowPrefab, _lineupSlotsContainer);
            row.gameObject.SetActive(true);
            row.Initialize(slot, _screenController);
        }

        _lineupSlotRowPrefab.gameObject.SetActive(false);
    }

    private void RenderEligiblePlayers(TeamData team, string selectedSlotType, string selectedSlotPosition, string selectedPlayerId)
    {
        ClearRows(_eligiblePlayersContainer, _eligiblePlayerRowPrefab == null ? null : _eligiblePlayerRowPrefab.transform);
        if (_eligiblePlayerRowPrefab == null)
        {
            return;
        }

        if (team == null || string.IsNullOrEmpty(selectedSlotType))
        {
            CreateInfoRow(_eligiblePlayersContainer, "Выберите слот в линиях");
            return;
        }

        List<PlayerData> players = LineupService.GetEligiblePlayersForSlot(team, selectedSlotType, selectedSlotPosition);
        if (players.Count == 0)
        {
            CreateInfoRow(_eligiblePlayersContainer, "Нет доступных игроков для выбранного слота");
            return;
        }

        foreach (PlayerData player in players)
        {
            LineupEligiblePlayerRowView row = Instantiate(_eligiblePlayerRowPrefab, _eligiblePlayersContainer);
            row.gameObject.SetActive(true);
            row.Initialize(player, _screenController);
        }

        _eligiblePlayerRowPrefab.gameObject.SetActive(false);
    }

    private void RenderScratches(TeamData team)
    {
        ClearRows(_scratchPlayersContainer, _scratchPlayerRowPrefab == null ? null : _scratchPlayerRowPrefab.transform);
        if (team == null || _scratchPlayerRowPrefab == null)
        {
            return;
        }

        List<PlayerData> scratches = LineupService.GetScratchPlayers(team);
        if (scratches.Count == 0)
        {
            CreateInfoRow(_scratchPlayersContainer, "Запасных игроков нет");
            return;
        }

        foreach (PlayerData player in scratches)
        {
            ScratchPlayerRowView row = Instantiate(_scratchPlayerRowPrefab, _scratchPlayersContainer);
            row.gameObject.SetActive(true);
            row.Initialize(player);
        }

        _scratchPlayerRowPrefab.gameObject.SetActive(false);
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

    private static void CreateInfoRow(Transform container, string value)
    {
        if (container == null)
        {
            return;
        }

        GameObject rowObject = new GameObject("InfoRow");
        rowObject.transform.SetParent(container, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 42f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 42f;
        layoutElement.minHeight = 42f;

        Text text = rowObject.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 16;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
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
}
