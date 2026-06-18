using System;
using System.Collections.Generic;

[Serializable]
public class TeamTradeProfileData
{
    public string TeamId;
    public string TeamName;
    public string Direction;
    public TeamNeedData Needs;
    public List<TradeBlockPlayerData> TradeBlock = new List<TradeBlockPlayerData>();
    public int BuyerScore;
    public int SellerScore;
    public int CapPressureScore;
    public int RosterPressureScore;
    public string UpdatedAtUtc = DateTime.UtcNow.ToString("o");

    public TeamTradeProfileData()
    {
        EnsureCollections();
    }

    public void EnsureCollections()
    {
        if (TradeBlock == null)
        {
            TradeBlock = new List<TradeBlockPlayerData>();
        }

        if (string.IsNullOrEmpty(UpdatedAtUtc))
        {
            UpdatedAtUtc = DateTime.UtcNow.ToString("o");
        }
    }
}
