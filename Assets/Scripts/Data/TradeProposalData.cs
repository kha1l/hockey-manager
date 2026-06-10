using System;
using System.Collections.Generic;

[Serializable]
public class TradeProposalData
{
    public string TradeId;
    public string FromTeamId;
    public string FromTeamName;
    public string ToTeamId;
    public string ToTeamName;
    public TradeAssetData PlayerFromUserTeam;
    public TradeAssetData PlayerFromOtherTeam;
    public List<TradeAssetData> AssetsFromUserTeam = new List<TradeAssetData>();
    public List<TradeAssetData> AssetsFromOtherTeam = new List<TradeAssetData>();
    public string CreatedAtUtc;
    public string Status;
    public string RejectionReason;

    public TradeProposalData()
    {
        EnsureAssets();
    }

    public void EnsureAssets()
    {
        if (AssetsFromUserTeam == null)
        {
            AssetsFromUserTeam = new List<TradeAssetData>();
        }

        if (AssetsFromOtherTeam == null)
        {
            AssetsFromOtherTeam = new List<TradeAssetData>();
        }
    }
}
