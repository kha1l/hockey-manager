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
        if (pick == null)
        {
            return 0;
        }

        return CalculateDraftRoundValue(pick.Round);
    }

    public static int CalculateAssetValue(TradeAssetData asset)
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
            return CalculateDraftRoundValue(asset.DraftRound);
        }

        return 0;
    }

    public static int CalculateAssetsValue(List<TradeAssetData> assets)
    {
        if (assets == null)
        {
            return 0;
        }

        int value = 0;
        foreach (TradeAssetData asset in assets)
        {
            value += CalculateAssetValue(asset);
        }

        return value;
    }

    private static int CalculatePlayerLikeValue(TradeAssetData asset)
    {
        int value = asset.Overall * 10;

        if (asset.Age <= 23)
        {
            value += asset.Overall * 3;
        }
        else if (asset.Age <= 30)
        {
            value += asset.Overall * 2;
        }
        else
        {
            value += asset.Overall;
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
}
