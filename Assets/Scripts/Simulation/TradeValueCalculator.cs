using System;
using System.Collections.Generic;

public static class TradeValueCalculator
{
    public static int CalculatePlayerTradeValue(PlayerData player)
    {
        if (player == null)
        {
            return 0;
        }

        int value = player.Overall * 10;

        if (player.Age <= 23)
        {
            value += player.Potential * 4;
        }
        else if (player.Age <= 30)
        {
            value += player.Potential * 2;
        }
        else
        {
            value += player.Potential;
        }

        if (player.ContractYearsRemaining <= 1)
        {
            value -= 75;
        }

        if (player.Salary > 10000000 && player.Overall < 85)
        {
            value -= 125;
        }

        if (player.HasNoTradeClause)
        {
            value -= 200;
        }

        return Math.Max(0, value);
    }

    public static int CalculateValueDifference(PlayerData a, PlayerData b)
    {
        return Math.Abs(CalculatePlayerTradeValue(a) - CalculatePlayerTradeValue(b));
    }

    public static int CalculateDraftPickValue(DraftPickOwnershipData pick)
    {
        return CalculateDraftPickValue(pick, null);
    }

    public static int CalculateDraftPickValue(DraftPickOwnershipData pick, GameState state)
    {
        if (pick == null)
        {
            return 0;
        }

        return ApplyDraftClassModifier(CalculateDraftRoundValue(pick.Round), pick.Round, pick.DraftYear, state);
    }

    public static int CalculateAssetValue(TradeAssetData asset)
    {
        return CalculateAssetValue(asset, null);
    }

    public static int CalculateAssetValue(TradeAssetData asset, GameState state)
    {
        if (asset == null)
        {
            return 0;
        }

        if (asset.AssetType == "Player")
        {
            return CalculatePlayerLikeValue(asset);
        }

        if (asset.AssetType == "DraftPick")
        {
            return ApplyDraftClassModifier(CalculateDraftRoundValue(asset.DraftRound), asset.DraftRound, asset.DraftYear, state);
        }

        return 0;
    }

    public static int CalculateAssetsValue(List<TradeAssetData> assets)
    {
        return CalculateAssetsValue(assets, null);
    }

    public static int CalculateAssetsValue(List<TradeAssetData> assets, GameState state)
    {
        if (assets == null)
        {
            return 0;
        }

        int value = 0;
        foreach (TradeAssetData asset in assets)
        {
            value += CalculateAssetValue(asset, state);
        }

        return value;
    }

    private static int CalculatePlayerLikeValue(TradeAssetData asset)
    {
        int value = asset.Overall * 10;

        int potential = asset.Potential > 0 ? asset.Potential : asset.Overall;
        if (asset.Age <= 23)
        {
            value += potential * 3;
        }
        else if (asset.Age <= 30)
        {
            value += potential * 2;
        }
        else
        {
            value += potential;
        }

        if (asset.ContractYearsRemaining <= 1)
        {
            value -= 75;
        }

        if (asset.Salary > 10000000 && asset.Overall < 85)
        {
            value -= 125;
        }

        if (asset.HasNoTradeClause)
        {
            value -= 200;
        }

        return Math.Max(0, value);
    }

    private static int CalculateDraftRoundValue(int round)
    {
        if (round == 1)
        {
            return 700;
        }

        if (round == 2)
        {
            return 350;
        }

        return round == 3 ? 175 : 0;
    }

    private static int ApplyDraftClassModifier(int baseValue, int round, int draftYear, GameState state)
    {
        if (baseValue <= 0 || state == null || state.Draft == null || state.Draft.ClassProfile == null)
        {
            return baseValue;
        }

        DraftClassProfileData profile = state.Draft.ClassProfile;
        if (profile.DraftYear > 0 && draftYear > 0 && profile.DraftYear != draftYear)
        {
            return baseValue;
        }

        int percent = 0;
        if (profile.StrengthType == DraftClassConfig.StrengthStrong)
        {
            percent += 10;
        }
        else if (profile.StrengthType == DraftClassConfig.StrengthWeak)
        {
            percent -= 10;
        }

        if (profile.DepthType == DraftClassConfig.DepthDeep)
        {
            percent += round == 1 ? 5 : 15;
        }
        else if (profile.DepthType == DraftClassConfig.DepthShallow)
        {
            percent -= round == 1 ? 5 : 15;
        }
        else if (profile.DepthType == DraftClassConfig.DepthTopHeavy)
        {
            percent += round == 1 ? 12 : -8;
        }

        int adjustedValue = baseValue + baseValue * percent / 100;
        return Math.Max(0, adjustedValue);
    }
}
