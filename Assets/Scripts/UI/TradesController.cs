using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TradesController : MonoBehaviour
{
    [SerializeField] private Text _dateText;
    [SerializeField] private Text _statusText;
    [SerializeField] private Text _selectedUserPlayerText;
    [SerializeField] private Text _selectedUserPickText;
    [SerializeField] private Text _selectedOtherTeamText;
    [SerializeField] private Text _selectedOtherPlayerText;
    [SerializeField] private Text _selectedOtherPickText;
    [SerializeField] private Text _tradePartnerNeedsText;
    [SerializeField] private Text _tradeAiDecisionText;
    [SerializeField] private Transform _userPlayersContainer;
    [SerializeField] private Transform _userPicksContainer;
    [SerializeField] private Transform _otherTeamsContainer;
    [SerializeField] private Transform _otherPlayersContainer;
    [SerializeField] private Transform _otherPicksContainer;
    [SerializeField] private Transform _tradeBlockContainer;
    [SerializeField] private Transform _historyContainer;
    [SerializeField] private TradePlayerRowView _userPlayerRowPrefab;
    [SerializeField] private TradeDraftPickRowView _userPickRowPrefab;
    [SerializeField] private TradeTeamRowView _teamRowPrefab;
    [SerializeField] private TradePlayerRowView _otherPlayerRowPrefab;
    [SerializeField] private TradeDraftPickRowView _otherPickRowPrefab;
    [SerializeField] private TradeBlockPlayerRowView _tradeBlockRowPrefab;
    [SerializeField] private TradeHistoryRowView _historyRowPrefab;
    [SerializeField] private GameScreenController _screenController;

    private GameState _state;
    private string _selectedUserPlayerId;
    private string _selectedUserPickId;
    private string _selectedOtherTeamId;
    private string _selectedOtherPlayerId;
    private string _selectedOtherPickId;

    public void Configure(
        Text dateText,
        Text statusText,
        Text selectedUserPlayerText,
        Text selectedUserPickText,
        Text selectedOtherTeamText,
        Text selectedOtherPlayerText,
        Text selectedOtherPickText,
        Text tradePartnerNeedsText,
        Text tradeAiDecisionText,
        Transform userPlayersContainer,
        Transform userPicksContainer,
        Transform otherTeamsContainer,
        Transform otherPlayersContainer,
        Transform otherPicksContainer,
        Transform tradeBlockContainer,
        Transform historyContainer,
        TradePlayerRowView userPlayerRowPrefab,
        TradeDraftPickRowView userPickRowPrefab,
        TradeTeamRowView teamRowPrefab,
        TradePlayerRowView otherPlayerRowPrefab,
        TradeDraftPickRowView otherPickRowPrefab,
        TradeBlockPlayerRowView tradeBlockRowPrefab,
        TradeHistoryRowView historyRowPrefab,
        GameScreenController screenController)
    {
        _dateText = dateText;
        _statusText = statusText;
        _selectedUserPlayerText = selectedUserPlayerText;
        _selectedUserPickText = selectedUserPickText;
        _selectedOtherTeamText = selectedOtherTeamText;
        _selectedOtherPlayerText = selectedOtherPlayerText;
        _selectedOtherPickText = selectedOtherPickText;
        _tradePartnerNeedsText = tradePartnerNeedsText;
        _tradeAiDecisionText = tradeAiDecisionText;
        _userPlayersContainer = userPlayersContainer;
        _userPicksContainer = userPicksContainer;
        _otherTeamsContainer = otherTeamsContainer;
        _otherPlayersContainer = otherPlayersContainer;
        _otherPicksContainer = otherPicksContainer;
        _tradeBlockContainer = tradeBlockContainer;
        _historyContainer = historyContainer;
        _userPlayerRowPrefab = userPlayerRowPrefab;
        _userPickRowPrefab = userPickRowPrefab;
        _teamRowPrefab = teamRowPrefab;
        _otherPlayerRowPrefab = otherPlayerRowPrefab;
        _otherPickRowPrefab = otherPickRowPrefab;
        _tradeBlockRowPrefab = tradeBlockRowPrefab;
        _historyRowPrefab = historyRowPrefab;
        _screenController = screenController;
    }

    public void ShowTrades(GameState state)
    {
        ShowTrades(state, "", "", "", "", "");
    }

    public void ShowTrades(
        GameState state,
        string selectedUserPlayerId,
        string selectedUserPickId,
        string selectedOtherTeamId,
        string selectedOtherPlayerId,
        string selectedOtherPickId)
    {
        _state = state;
        _selectedUserPlayerId = selectedUserPlayerId;
        _selectedUserPickId = selectedUserPickId;
        _selectedOtherTeamId = selectedOtherTeamId;
        _selectedOtherPlayerId = selectedOtherPlayerId;
        _selectedOtherPickId = selectedOtherPickId;

        if (!HasRequiredReferences())
        {
            Debug.LogError("TradesController: UI references are not configured.");
            return;
        }

        RenderDateAndStatus();
        RenderSelectedItems();
        RenderUserPlayers();
        RenderUserPicks();
        RenderTeams();
        RenderOtherTeamPlayers();
        RenderOtherTeamPicks();
        RenderTradePartnerProfile();
        RenderTradeBlock();
        RenderAiDecision();
        RenderHistory();
    }

    public void RefreshOtherTeamPlayers(string otherTeamId)
    {
        _selectedOtherTeamId = otherTeamId;
        RenderOtherTeamPlayers();
        RenderOtherTeamPicks();
        RenderTradePartnerProfile();
        RenderTradeBlock();
        RenderAiDecision();
    }

    private void RenderDateAndStatus()
    {
        DateTime currentDate = LeagueDateService.GetCurrentLeagueDate(_state);
        string deadline = _state == null || _state.LeagueCalendar == null
            ? ""
            : _state.LeagueCalendar.TradeDeadlineDate;

        _dateText.text = "Дата лиги: " + currentDate.ToString("yyyy-MM-dd")
            + "\nTrade deadline: " + deadline;
        _statusText.text = LeagueDateService.IsPastTradeDeadline(_state)
            ? "Обмены после дедлайна недоступны"
            : "Обмены доступны";
    }

    private void RenderSelectedItems()
    {
        TeamData userTeam = GetUserTeam();
        TeamData otherTeam = FindTeam(_selectedOtherTeamId);
        PlayerData userPlayer = FindPlayer(userTeam, _selectedUserPlayerId);
        PlayerData otherPlayer = FindPlayer(otherTeam, _selectedOtherPlayerId);
        DraftPickOwnershipData userPick = DraftPickOwnershipService.FindPick(_state, _selectedUserPickId);
        DraftPickOwnershipData otherPick = DraftPickOwnershipService.FindPick(_state, _selectedOtherPickId);

        _selectedUserPlayerText.text = userPlayer == null
            ? "Ваш игрок: Выберите игрока"
            : "Ваш игрок: " + GetPlayerName(userPlayer);
        _selectedUserPickText.text = userPick == null
            ? "Ваш пик: Выберите пик"
            : "Ваш пик: " + GetPickName(userPick);
        _selectedOtherTeamText.text = otherTeam == null
            ? "Команда-соперник: Выберите команду для обмена"
            : "Команда-соперник: " + GetTeamName(otherTeam);
        _selectedOtherPlayerText.text = otherPlayer == null
            ? "Игрок соперника: Выберите игрока"
            : "Игрок соперника: " + GetPlayerName(otherPlayer);
        _selectedOtherPickText.text = otherPick == null
            ? "Пик соперника: Выберите пик"
            : "Пик соперника: " + GetPickName(otherPick);
    }

    private void RenderUserPicks()
    {
        ClearRows(_userPicksContainer, _userPickRowPrefab.transform);
        _userPickRowPrefab.gameObject.SetActive(false);

        TeamData userTeam = GetUserTeam();
        if (userTeam == null)
        {
            CreateInfoRow(_userPicksContainer, "Команда пользователя не найдена");
            return;
        }

        List<DraftPickOwnershipData> picks = DraftPickOwnershipService.GetOwnedPicks(_state, userTeam.Id);
        if (picks.Count == 0)
        {
            CreateInfoRow(_userPicksContainer, "Доступных пиков нет");
            return;
        }

        foreach (DraftPickOwnershipData pick in picks)
        {
            TradeDraftPickRowView row = Instantiate(_userPickRowPrefab, _userPicksContainer);
            row.name = pick.PickId + "-trade-user-pick-row";
            row.gameObject.SetActive(true);
            row.InitializeUserPick(pick, _screenController);
        }
    }

    private void RenderUserPlayers()
    {
        ClearRows(_userPlayersContainer, _userPlayerRowPrefab.transform);
        _userPlayerRowPrefab.gameObject.SetActive(false);

        TeamData userTeam = GetUserTeam();
        EnsureTeamPlayers(userTeam);
        if (userTeam == null)
        {
            CreateInfoRow(_userPlayersContainer, "Команда пользователя не найдена");
            return;
        }

        int shown = UiDisplayLimitConfig.ClampRowCount(userTeam.Players.Count, UiDisplayLimitConfig.MaxRosterRows);
        for (int i = 0; i < shown; i++)
        {
            PlayerData player = userTeam.Players[i];
            TradePlayerRowView row = Instantiate(_userPlayerRowPrefab, _userPlayersContainer);
            row.name = player.Id + "-trade-user-row";
            row.gameObject.SetActive(true);
            row.InitializeUserPlayer(player, _screenController);
        }

        string limitMessage = UiDisplayLimitConfig.BuildLimitMessage(shown, userTeam.Players.Count);
        if (!string.IsNullOrEmpty(limitMessage))
        {
            CreateInfoRow(_userPlayersContainer, limitMessage);
        }
    }

    private void RenderOtherTeamPicks()
    {
        ClearRows(_otherPicksContainer, _otherPickRowPrefab.transform);
        _otherPickRowPrefab.gameObject.SetActive(false);

        TeamData otherTeam = FindTeam(_selectedOtherTeamId);
        if (otherTeam == null)
        {
            CreateInfoRow(_otherPicksContainer, "Выберите команду для обмена");
            return;
        }

        List<DraftPickOwnershipData> picks = DraftPickOwnershipService.GetOwnedPicks(_state, otherTeam.Id);
        if (picks.Count == 0)
        {
            CreateInfoRow(_otherPicksContainer, "Доступных пиков нет");
            return;
        }

        foreach (DraftPickOwnershipData pick in picks)
        {
            TradeDraftPickRowView row = Instantiate(_otherPickRowPrefab, _otherPicksContainer);
            row.name = pick.PickId + "-trade-other-pick-row";
            row.gameObject.SetActive(true);
            row.InitializeOtherPick(pick, _screenController);
        }
    }

    private void RenderTeams()
    {
        ClearRows(_otherTeamsContainer, _teamRowPrefab.transform);
        _teamRowPrefab.gameObject.SetActive(false);

        if (_state == null || _state.Teams == null)
        {
            CreateInfoRow(_otherTeamsContainer, "Команды не найдены");
            return;
        }

        foreach (TeamData team in _state.Teams)
        {
            if (team == null || team.Id == _state.SelectedTeamId)
            {
                continue;
            }

            EnsureTeamPlayers(team);
            TradeTeamRowView row = Instantiate(_teamRowPrefab, _otherTeamsContainer);
            row.name = team.Id + "-trade-team-row";
            row.gameObject.SetActive(true);
            row.Initialize(team, _screenController);
        }
    }

    private void RenderOtherTeamPlayers()
    {
        ClearRows(_otherPlayersContainer, _otherPlayerRowPrefab.transform);
        _otherPlayerRowPrefab.gameObject.SetActive(false);

        TeamData otherTeam = FindTeam(_selectedOtherTeamId);
        EnsureTeamPlayers(otherTeam);
        if (otherTeam == null)
        {
            CreateInfoRow(_otherPlayersContainer, "Выберите команду для обмена");
            return;
        }

        int shown = UiDisplayLimitConfig.ClampRowCount(otherTeam.Players.Count, UiDisplayLimitConfig.MaxRosterRows);
        for (int i = 0; i < shown; i++)
        {
            PlayerData player = otherTeam.Players[i];
            TradePlayerRowView row = Instantiate(_otherPlayerRowPrefab, _otherPlayersContainer);
            row.name = player.Id + "-trade-other-row";
            row.gameObject.SetActive(true);
            row.InitializeOtherPlayer(player, _screenController);
        }

        string limitMessage = UiDisplayLimitConfig.BuildLimitMessage(shown, otherTeam.Players.Count);
        if (!string.IsNullOrEmpty(limitMessage))
        {
            CreateInfoRow(_otherPlayersContainer, limitMessage);
        }
    }

    private void RenderTradePartnerProfile()
    {
        if (_tradePartnerNeedsText == null)
        {
            return;
        }

        TeamTradeProfileService.EnsureTradeProfiles(_state);
        TeamTradeProfileData profile = TeamTradeProfileService.GetTradeProfile(_state, _selectedOtherTeamId);
        if (profile == null || profile.Needs == null)
        {
            _tradePartnerNeedsText.text = "Team needs: выберите CPU-команду";
            return;
        }

        _tradePartnerNeedsText.text = "Team needs: " + profile.TeamName
            + " | " + profile.Direction
            + " | Need: " + profile.Needs.PrimaryNeed + " / " + profile.Needs.SecondaryNeed
            + "\nBuyer " + profile.BuyerScore
            + " | Seller " + profile.SellerScore
            + " | Cap pressure " + profile.CapPressureScore
            + " | Roster pressure " + profile.RosterPressureScore;
    }

    private void RenderTradeBlock()
    {
        if (_tradeBlockContainer == null || _tradeBlockRowPrefab == null)
        {
            return;
        }

        ClearRows(_tradeBlockContainer, _tradeBlockRowPrefab.transform);
        _tradeBlockRowPrefab.gameObject.SetActive(false);

        TeamTradeProfileData profile = TeamTradeProfileService.GetTradeProfile(_state, _selectedOtherTeamId);
        if (profile == null || profile.TradeBlock == null || profile.TradeBlock.Count == 0)
        {
            CreateInfoRow(_tradeBlockContainer, "Trade block пуст");
            return;
        }

        int shown = UiDisplayLimitConfig.ClampRowCount(profile.TradeBlock.Count, UiDisplayLimitConfig.MaxTradeBlockRows);
        for (int i = 0; i < shown; i++)
        {
            TradeBlockPlayerData player = profile.TradeBlock[i];
            TradeBlockPlayerRowView row = Instantiate(_tradeBlockRowPrefab, _tradeBlockContainer);
            row.name = player.PlayerId + "-trade-block-row";
            row.gameObject.SetActive(true);
            row.Initialize(player, _screenController);
        }

        string limitMessage = UiDisplayLimitConfig.BuildLimitMessage(shown, profile.TradeBlock.Count);
        if (!string.IsNullOrEmpty(limitMessage))
        {
            CreateInfoRow(_tradeBlockContainer, limitMessage);
        }
    }

    private void RenderAiDecision()
    {
        if (_tradeAiDecisionText == null)
        {
            return;
        }

        TradeProposalData proposal = GetLastTradeProposal();
        if (proposal == null)
        {
            _tradeAiDecisionText.text = "AI decision: предложений пока нет";
            return;
        }

        string reason = string.IsNullOrEmpty(proposal.AiDecisionReason)
            ? proposal.RejectionReason
            : proposal.AiDecisionReason;
        _tradeAiDecisionText.text = "AI decision: " + proposal.Status
            + " | score " + proposal.AiAcceptanceScore
            + "\n" + reason;
    }

    private void RenderHistory()
    {
        ClearRows(_historyContainer, _historyRowPrefab.transform);
        _historyRowPrefab.gameObject.SetActive(false);

        TradeService.EnsureTradeHistory(_state);
        if (_state == null || _state.TradeHistory == null || _state.TradeHistory.Trades.Count == 0)
        {
            CreateInfoRow(_historyContainer, "История обменов пуста");
            return;
        }

        int firstIndex = Mathf.Max(0, _state.TradeHistory.Trades.Count - 10);
        for (int i = _state.TradeHistory.Trades.Count - 1; i >= firstIndex; i--)
        {
            TradeHistoryRowView row = Instantiate(_historyRowPrefab, _historyContainer);
            row.name = "trade-history-" + i;
            row.gameObject.SetActive(true);
            row.Initialize(_state.TradeHistory.Trades[i]);
        }
    }

    private bool HasRequiredReferences()
    {
        return _dateText != null
            && _statusText != null
            && _selectedUserPlayerText != null
            && _selectedUserPickText != null
            && _selectedOtherTeamText != null
            && _selectedOtherPlayerText != null
            && _selectedOtherPickText != null
            && _tradePartnerNeedsText != null
            && _tradeAiDecisionText != null
            && _userPlayersContainer != null
            && _userPicksContainer != null
            && _otherTeamsContainer != null
            && _otherPlayersContainer != null
            && _otherPicksContainer != null
            && _tradeBlockContainer != null
            && _historyContainer != null
            && _userPlayerRowPrefab != null
            && _userPickRowPrefab != null
            && _teamRowPrefab != null
            && _otherPlayerRowPrefab != null
            && _otherPickRowPrefab != null
            && _tradeBlockRowPrefab != null
            && _historyRowPrefab != null;
    }

    private TradeProposalData GetLastTradeProposal()
    {
        if (_state == null || _state.TradeHistory == null || _state.TradeHistory.Trades == null || _state.TradeHistory.Trades.Count == 0)
        {
            return null;
        }

        for (int i = _state.TradeHistory.Trades.Count - 1; i >= 0; i--)
        {
            TradeProposalData proposal = _state.TradeHistory.Trades[i];
            if (proposal != null)
            {
                return proposal;
            }
        }

        return null;
    }

    private TeamData GetUserTeam()
    {
        return _state == null ? null : FindTeam(_state.SelectedTeamId);
    }

    private TeamData FindTeam(string teamId)
    {
        if (_state == null || _state.Teams == null || string.IsNullOrEmpty(teamId))
        {
            return null;
        }

        foreach (TeamData team in _state.Teams)
        {
            if (team != null && team.Id == teamId)
            {
                return team;
            }
        }

        return null;
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
            if (player != null && !player.IsRetired && player.Id == playerId)
            {
                return player;
            }
        }

        return null;
    }

    private static void EnsureTeamPlayers(TeamData team)
    {
        if (team == null)
        {
            return;
        }

        team.EnsurePlayers();
        if (team.Players.Count == 0)
        {
            team.Players = PlayerSeedData.CreatePlayersForTeam(team.Id);
        }

        ContractGenerator.EnsureContractsForTeam(team);
    }

    private static void ClearRows(Transform container, Transform prefabTransform)
    {
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Transform child = container.GetChild(i);
            if (child == prefabTransform)
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }

    private static void CreateInfoRow(Transform container, string value)
    {
        GameObject rowObject = new GameObject("InfoRow");
        rowObject.transform.SetParent(container, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(520f, 44f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 44f;
        layoutElement.minHeight = 44f;

        Text text = rowObject.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 16;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
    }

    private static string GetPlayerName(PlayerData player)
    {
        return player.FirstName + " " + player.LastName;
    }

    private static string GetTeamName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }

    private static string GetPickName(DraftPickOwnershipData pick)
    {
        return pick.DraftYear + " Round " + pick.Round + " from " + pick.OriginalTeamName;
    }
}
