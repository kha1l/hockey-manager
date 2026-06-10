using System;
using System.Collections.Generic;

[Serializable]
public class TradeHistoryData
{
    public List<TradeProposalData> Trades = new List<TradeProposalData>();

    public TradeHistoryData()
    {
        EnsureTrades();
    }

    public void EnsureTrades()
    {
        if (Trades == null)
        {
            Trades = new List<TradeProposalData>();
        }
    }
}
