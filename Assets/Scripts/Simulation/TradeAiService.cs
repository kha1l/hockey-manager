using System;
using System.Collections.Generic;

public static class TradeAiService
{
    public static bool ShouldCpuAcceptTrade(
        GameState state,
        TeamData cpuTeam,
        List<TradeAssetData> assetsCpuReceives,
        List<TradeAssetData> assetsCpuGives,
        out TradeFitEvaluationData evaluation)
    {
        TeamTradeProfileService.EnsureTradeProfiles(state);
        evaluation = TradeFitEvaluator.EvaluateTradeForTeam(
            state,
            cpuTeam,
            assetsCpuReceives,
            assetsCpuGives);
        return evaluation != null && evaluation.Accepted;
    }

    public static List<TradeProposalData> GenerateCpuTradeIdeas(
        GameState state,
        TeamData fromTeam,
        int maxIdeas)
    {
        List<TradeProposalData> ideas = new List<TradeProposalData>();
        if (state == null || fromTeam == null || maxIdeas <= 0)
        {
            return ideas;
        }

        TeamTradeProfileService.EnsureTradeProfiles(state);
        List<TeamData> partners = FindPotentialTradePartners(state, fromTeam);
        foreach (TeamData partner in partners)
        {
            TradeProposalData idea = CreateSimpleTradeIdea(state, fromTeam, partner);
            if (idea != null)
            {
                ideas.Add(idea);
            }

            if (ideas.Count >= maxIdeas)
            {
                break;
            }
        }

        return ideas;
    }

    public static TradeProposalData CreateSimpleTradeIdea(
        GameState state,
        TeamData buyer,
        TeamData seller)
    {
        TeamTradeProfileData sellerProfile = TeamTradeProfileService.GetTradeProfile(state, seller == null ? "" : seller.Id);
        if (state == null || buyer == null || seller == null || sellerProfile == null || sellerProfile.TradeBlock == null || sellerProfile.TradeBlock.Count == 0)
        {
            return null;
        }

        TradeBlockPlayerData blockPlayer = sellerProfile.TradeBlock[0];
        PlayerData sellingPlayer = FindPlayer(seller, blockPlayer.PlayerId);
        TradeAssetData sellingAsset = TradeService.CreatePlayerAsset(sellingPlayer, seller);
        TradeAssetData buyerAsset = FindBuyerDraftPickAsset(state, buyer);
        if (sellingAsset == null || buyerAsset == null)
        {
            return null;
        }

        TradeProposalData proposal = new TradeProposalData
        {
            TradeId = Guid.NewGuid().ToString("N"),
            FromTeamId = buyer.Id,
            FromTeamName = TeamIdentityService.GetDisplayName(buyer),
            ToTeamId = seller.Id,
            ToTeamName = TeamIdentityService.GetDisplayName(seller),
            CreatedAtUtc = DateTime.UtcNow.ToString("o"),
            Status = "Idea",
            AssetsFromUserTeam = new List<TradeAssetData> { buyerAsset },
            AssetsFromOtherTeam = new List<TradeAssetData> { sellingAsset }
        };

        proposal.UserTeamEvaluation = TradeFitEvaluator.EvaluateTradeForTeam(state, buyer, proposal.AssetsFromOtherTeam, proposal.AssetsFromUserTeam);
        proposal.OtherTeamEvaluation = TradeFitEvaluator.EvaluateTradeForTeam(state, seller, proposal.AssetsFromUserTeam, proposal.AssetsFromOtherTeam);
        proposal.AiDecisionReason = proposal.OtherTeamEvaluation == null ? "" : proposal.OtherTeamEvaluation.Reason;
        proposal.AiAcceptanceScore = proposal.OtherTeamEvaluation == null ? 0 : proposal.OtherTeamEvaluation.FinalScore;
        return proposal;
    }

    public static List<TeamData> FindPotentialTradePartners(
        GameState state,
        TeamData team)
    {
        List<TeamData> partners = new List<TeamData>();
        if (state == null || state.Teams == null || team == null)
        {
            return partners;
        }

        TeamTradeProfileData profile = TeamTradeProfileService.GetTradeProfile(state, team.Id);
        foreach (TeamData otherTeam in state.Teams)
        {
            if (otherTeam == null || otherTeam.Id == team.Id)
            {
                continue;
            }

            TeamTradeProfileData otherProfile = TeamTradeProfileService.GetTradeProfile(state, otherTeam.Id);
            if (otherProfile == null)
            {
                continue;
            }

            bool teamIsBuyer = profile != null && profile.BuyerScore >= profile.SellerScore;
            bool partnerFits = teamIsBuyer ? otherProfile.SellerScore >= 45 : otherProfile.BuyerScore >= 45;
            if (partnerFits)
            {
                partners.Add(otherTeam);
            }
        }

        partners.Sort(delegate(TeamData left, TeamData right)
        {
            TeamTradeProfileData leftProfile = TeamTradeProfileService.GetTradeProfile(state, left.Id);
            TeamTradeProfileData rightProfile = TeamTradeProfileService.GetTradeProfile(state, right.Id);
            int leftScore = leftProfile == null ? 0 : leftProfile.BuyerScore + leftProfile.SellerScore;
            int rightScore = rightProfile == null ? 0 : rightProfile.BuyerScore + rightProfile.SellerScore;
            return rightScore.CompareTo(leftScore);
        });
        return partners;
    }

    private static TradeAssetData FindBuyerDraftPickAsset(GameState state, TeamData buyer)
    {
        List<DraftPickOwnershipData> picks = DraftPickOwnershipService.GetOwnedPicks(state, buyer == null ? "" : buyer.Id);
        if (picks == null || picks.Count == 0)
        {
            return null;
        }

        picks.Sort(delegate(DraftPickOwnershipData left, DraftPickOwnershipData right)
        {
            int comparison = left.Round.CompareTo(right.Round);
            return comparison != 0 ? comparison : left.DraftYear.CompareTo(right.DraftYear);
        });

        return TradeService.CreateDraftPickAsset(picks[0]);
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
}
