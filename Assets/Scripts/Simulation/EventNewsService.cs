using System.Collections.Generic;

public static class EventNewsService
{
    public static void CreateTradeNews(GameState state, TradeHistoryData trade)
    {
        if (state == null || trade == null || trade.Trades == null)
        {
            return;
        }

        for (int i = trade.Trades.Count - 1; i >= 0; i--)
        {
            TradeProposalData proposal = trade.Trades[i];
            if (proposal != null && proposal.Status == "Accepted")
            {
                CreateTradeNews(state, proposal);
                return;
            }
        }
    }

    public static void CreateTradeNews(GameState state, TradeProposalData proposal)
    {
        if (state == null || proposal == null || proposal.Status != "Accepted")
        {
            return;
        }

        proposal.EnsureAssets();
        bool userTrade = proposal.FromTeamId == state.SelectedTeamId || proposal.ToTeamId == state.SelectedTeamId;
        bool starTrade = HasMajorPlayer(proposal.AssetsFromUserTeam) || HasMajorPlayer(proposal.AssetsFromOtherTeam);
        bool firstRoundPick = HasFirstRoundPick(proposal.AssetsFromUserTeam) || HasFirstRoundPick(proposal.AssetsFromOtherTeam);
        if (!userTrade && !starTrade && !firstRoundPick)
        {
            return;
        }

        int importance = userTrade ? 80 : starTrade ? 85 : 70;
        string title = starTrade || firstRoundPick ? "Major trade completed" : proposal.FromTeamName + " completes trade";
        string body = SafeText(proposal.FromTeamName, "Team")
            + " and " + SafeText(proposal.ToTeamName, "Team")
            + " completed a trade. "
            + "Assets moved: " + FormatAssets(proposal.AssetsFromUserTeam)
            + " for " + FormatAssets(proposal.AssetsFromOtherTeam) + ".";

        NewsFeedService.AddNews(
            state,
            NewsConfig.CategoryTrade,
            title,
            body,
            importance,
            proposal.FromTeamId,
            proposal.FromTeamName,
            "",
            "",
            proposal.TradeId);
    }

    public static void CreateMajorInjuryNews(GameState state, TeamData team, PlayerData player)
    {
        if (state == null || team == null || player == null)
        {
            return;
        }

        bool userTeam = team.Id == state.SelectedTeamId;
        if (!userTeam && player.Overall < 80 && player.InjuryDaysRemaining < 21)
        {
            return;
        }

        int importance = player.InjuryDaysRemaining >= 45 || player.Overall >= 85 ? 85 : userTeam ? 75 : 60;
        string playerName = GetPlayerName(player);
        NewsFeedService.AddNews(
            state,
            NewsConfig.CategoryInjury,
            playerName + " sidelined with injury",
            playerName + " will miss around " + player.InjuryDaysRemaining + " days with " + SafeText(player.InjuryType, "an injury") + ".",
            importance,
            team.Id,
            GetTeamName(team),
            player.Id,
            playerName,
            "Injury_" + player.Id + "_" + player.InjuredAtUtc);
    }

    public static void CreateContractExtensionNews(GameState state, ContractExtensionOfferData offer)
    {
        if (state == null || offer == null || !offer.Accepted)
        {
            return;
        }

        PlayerData player = FindPlayer(state, offer.PlayerId);
        bool userTeam = offer.TeamId == state.SelectedTeamId;
        if (!userTeam && offer.OfferedSalary < 5000000 && offer.OfferedYears < 4 && (player == null || player.Overall < 78))
        {
            return;
        }

        NewsFeedService.AddNews(
            state,
            NewsConfig.CategoryContract,
            SafeText(offer.PlayerName, "Player") + " signs extension with " + SafeText(offer.TeamName, "team"),
            SafeText(offer.PlayerName, "Player") + " agreed to a " + offer.OfferedYears + "-year extension worth " + FormatMoney(offer.OfferedSalary) + " per season.",
            userTeam ? 75 : 65,
            offer.TeamId,
            offer.TeamName,
            offer.PlayerId,
            offer.PlayerName,
            offer.OfferId);
    }

    public static void CreateFreeAgentSigningNews(GameState state, FreeAgentOfferData offer)
    {
        if (state == null || offer == null || !offer.Accepted)
        {
            return;
        }

        PlayerData player = FindPlayer(state, offer.PlayerId);
        bool userTeam = offer.TeamId == state.SelectedTeamId;
        if (!userTeam && !IsMajorSigning(offer.OfferedSalary, offer.OfferedYears) && (player == null || player.Overall < 78))
        {
            return;
        }

        NewsFeedService.AddNews(
            state,
            NewsConfig.CategoryFreeAgency,
            SafeText(offer.TeamName, "Team") + " signs " + SafeText(offer.PlayerName, "free agent"),
            SafeText(offer.PlayerName, "The player") + " agreed to a " + offer.OfferedYears + "-year deal worth " + FormatMoney(offer.OfferedSalary) + " per season.",
            userTeam ? 75 : 65,
            offer.TeamId,
            offer.TeamName,
            offer.PlayerId,
            offer.PlayerName,
            offer.OfferId);
    }

    public static void CreateFreeAgentSigningNews(GameState state, FreeAgentSigningData signing)
    {
        if (state == null || signing == null || signing.Status != "Accepted")
        {
            return;
        }

        PlayerData player = FindPlayer(state, signing.PlayerId);
        bool userTeam = signing.TeamId == state.SelectedTeamId;
        if (!userTeam && !IsMajorSigning(signing.Salary, signing.ContractYears) && (player == null || player.Overall < 78))
        {
            return;
        }

        NewsFeedService.AddNews(
            state,
            NewsConfig.CategoryFreeAgency,
            SafeText(signing.TeamName, "Team") + " signs " + SafeText(signing.PlayerName, "free agent"),
            SafeText(signing.PlayerName, "The player") + " agreed to a " + signing.ContractYears + "-year deal worth " + FormatMoney(signing.Salary) + " per season.",
            userTeam ? 75 : 65,
            signing.TeamId,
            signing.TeamName,
            signing.PlayerId,
            signing.PlayerName,
            signing.SigningId);
    }

    public static void CreateDraftPickNews(GameState state, DraftPickData pick)
    {
        if (state == null || pick == null || !pick.IsCompleted)
        {
            return;
        }

        bool userPick = pick.TeamId == state.SelectedTeamId || pick.IsUserTeamPick;
        if (!userPick && pick.OverallPick > 10)
        {
            return;
        }

        NewsFeedService.AddNews(
            state,
            NewsConfig.CategoryDraft,
            SafeText(pick.TeamName, "Team") + " drafts " + SafeText(pick.SelectedProspectName, "a prospect"),
            SafeText(pick.SelectedProspectName, "The prospect") + " was selected #" + pick.OverallPick + " in round " + pick.Round + ".",
            pick.OverallPick <= 3 ? 85 : userPick ? 75 : 65,
            pick.TeamId,
            pick.TeamName,
            pick.SelectedProspectId,
            pick.SelectedProspectName,
            pick.PickId);
    }

    public static void CreateDevelopmentBreakoutNews(GameState state, TeamData team, PlayerData player, int developmentDelta)
    {
        if (state == null || team == null || player == null)
        {
            return;
        }

        if (developmentDelta < 4 && player.Overall < 80 && !(team.Id == state.SelectedTeamId && player.Potential >= 84 && developmentDelta >= 2))
        {
            return;
        }

        string playerName = GetPlayerName(player);
        NewsFeedService.AddNews(
            state,
            NewsConfig.CategoryDevelopment,
            playerName + " makes major development leap",
            playerName + " improved by " + FormatSigned(developmentDelta) + " overall and is now rated " + player.Overall + ".",
            team.Id == state.SelectedTeamId ? 70 : 55,
            team.Id,
            GetTeamName(team),
            player.Id,
            playerName,
            "Development_" + state.CurrentSeasonStartYear + "_" + player.Id);
    }

    public static void CreateTradeRequestNews(GameState state, TeamData team, PlayerData player)
    {
        if (state == null || team == null || player == null || !player.WantsTrade)
        {
            return;
        }

        string playerName = GetPlayerName(player);
        NewsFeedService.AddNews(
            state,
            NewsConfig.CategoryTeam,
            playerName + " requests a trade",
            playerName + " has requested a trade after sustained low morale.",
            75,
            team.Id,
            GetTeamName(team),
            player.Id,
            playerName,
            "TradeRequest_" + player.Id + "_" + state.CurrentSeasonStartYear);
    }

    public static void CreateGmFiringNews(GameState state, GmCareerEventData eventData)
    {
        if (state == null || eventData == null)
        {
            return;
        }

        NewsFeedService.AddNews(
            state,
            NewsConfig.CategoryOwner,
            "GM fired by " + SafeText(eventData.TeamName, "team"),
            SafeText(eventData.Summary, "The club has decided to make a change in the GM chair."),
            90,
            eventData.TeamId,
            eventData.TeamName,
            "",
            "",
            eventData.EventId);
    }

    public static void CreateGmHiringNews(GameState state, GmCareerEventData eventData)
    {
        if (state == null || eventData == null)
        {
            return;
        }

        NewsFeedService.AddNews(
            state,
            NewsConfig.CategoryOwner,
            "GM hired by " + SafeText(eventData.TeamName, "team"),
            SafeText(eventData.Summary, "The club has hired a new GM."),
            80,
            eventData.TeamId,
            eventData.TeamName,
            "",
            "",
            eventData.EventId);
    }

    public static void CreateGmWarningNews(GameState state, GmCareerEventData eventData)
    {
        if (state == null || eventData == null)
        {
            return;
        }

        NewsFeedService.AddNews(
            state,
            NewsConfig.CategoryOwner,
            "Owner puts GM under pressure",
            SafeText(eventData.Summary, "Ownership expects improvement soon."),
            70,
            eventData.TeamId,
            eventData.TeamName,
            "",
            "",
            eventData.EventId);
    }

    public static void CreateRetirementNews(GameState state, RetiredPlayerData player)
    {
        if (state == null || player == null)
        {
            return;
        }

        bool userTeam = player.LastTeamId == state.SelectedTeamId || player.PrimaryTeamId == state.SelectedTeamId;
        bool notable = player.HallOfFameScore >= 70
            || player.CareerGamesPlayed >= 700
            || player.CareerAwardsCount > 0
            || userTeam;
        if (!notable)
        {
            return;
        }

        int importance = player.HallOfFameScore >= RetirementConfig.HallOfFameScoreThreshold ? 85 : userTeam ? 75 : 60;
        NewsFeedService.AddNews(
            state,
            userTeam ? NewsConfig.CategoryTeam : NewsConfig.CategoryMilestone,
            SafeText(player.PlayerName, "Player") + " announces retirement",
            SafeText(player.CareerSummary, SafeText(player.PlayerName, "The player") + " has retired after a veteran career."),
            importance,
            string.IsNullOrEmpty(player.LastTeamId) ? player.PrimaryTeamId : player.LastTeamId,
            string.IsNullOrEmpty(player.LastTeamName) ? player.PrimaryTeamName : player.LastTeamName,
            player.PlayerId,
            player.PlayerName,
            "Retirement_" + player.PlayerId);
    }

    public static void CreateHallOfFameNews(GameState state, HallOfFameInducteeData inductee)
    {
        if (state == null || inductee == null)
        {
            return;
        }

        NewsFeedService.AddNews(
            state,
            NewsConfig.CategoryMilestone,
            SafeText(inductee.PlayerName, "Player") + " inducted into Hall of Fame",
            SafeText(inductee.InductionSummary, "A decorated career has been honored by the league."),
            95,
            inductee.PrimaryTeamId,
            inductee.PrimaryTeamName,
            inductee.PlayerId,
            inductee.PlayerName,
            "HOF_" + inductee.PlayerId);
    }

    public static void CreateRetiredNumberNews(GameState state, RetiredNumberData retiredNumber)
    {
        if (state == null || retiredNumber == null)
        {
            return;
        }

        NewsFeedService.AddNews(
            state,
            NewsConfig.CategoryTeam,
            SafeText(retiredNumber.TeamName, "Team") + " retires #" + retiredNumber.JerseyNumber + " for " + SafeText(retiredNumber.PlayerName, "Player"),
            SafeText(retiredNumber.Reason, "The club has retired the number after a legendary career."),
            90,
            retiredNumber.TeamId,
            retiredNumber.TeamName,
            retiredNumber.PlayerId,
            retiredNumber.PlayerName,
            "RetiredNumber_" + retiredNumber.TeamId + "_" + retiredNumber.JerseyNumber);
    }

    public static bool IsMajorPlayer(PlayerData player)
    {
        return player != null && player.Overall >= 80;
    }

    public static bool IsMajorSigning(int salary, int years)
    {
        return salary >= 4000000 || years >= 3;
    }

    private static bool HasMajorPlayer(List<TradeAssetData> assets)
    {
        if (assets == null)
        {
            return false;
        }

        foreach (TradeAssetData asset in assets)
        {
            if (asset != null && asset.AssetType == "Player" && asset.Overall >= 80)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasFirstRoundPick(List<TradeAssetData> assets)
    {
        if (assets == null)
        {
            return false;
        }

        foreach (TradeAssetData asset in assets)
        {
            if (asset != null && asset.AssetType == "DraftPick" && asset.DraftRound == 1)
            {
                return true;
            }
        }

        return false;
    }

    private static string FormatAssets(List<TradeAssetData> assets)
    {
        if (assets == null || assets.Count == 0)
        {
            return "none";
        }

        string text = "";
        int shown = 0;
        foreach (TradeAssetData asset in assets)
        {
            if (asset == null)
            {
                continue;
            }

            if (shown > 0)
            {
                text += ", ";
            }

            text += asset.AssetType == "DraftPick"
                ? "Round " + asset.DraftRound + " pick"
                : SafeText(asset.PlayerName, "Player");
            shown++;
        }

        return string.IsNullOrEmpty(text) ? "none" : text;
    }

    private static PlayerData FindPlayer(GameState state, string playerId)
    {
        if (state == null || string.IsNullOrEmpty(playerId))
        {
            return null;
        }

        foreach (PlayerData player in CareerStatsService.GetAllPlayersIncludingFreeAgents(state))
        {
            if (player != null && player.Id == playerId)
            {
                return player;
            }
        }

        return null;
    }

    private static string GetPlayerName(PlayerData player)
    {
        return player == null ? "" : player.FirstName + " " + player.LastName;
    }

    private static string GetTeamName(TeamData team)
    {
        return TeamIdentityService.GetDisplayName(team);
    }

    private static string FormatMoney(int value)
    {
        return value.ToString("N0").Replace(",", " ");
    }

    private static string FormatSigned(int value)
    {
        return value >= 0 ? "+" + value : value.ToString();
    }

    private static string SafeText(string value, string fallback)
    {
        return string.IsNullOrEmpty(value) ? fallback : value;
    }
}
