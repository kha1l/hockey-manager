using System;
using System.Collections.Generic;

[Serializable]
public class ContractExtensionHistoryData
{
    public List<ContractExtensionOfferData> Offers = new List<ContractExtensionOfferData>();
    public int TotalOffers;
    public int AcceptedOffers;
    public int RejectedOffers;
    public string LastOfferAtUtc;

    public void EnsureOffers()
    {
        if (Offers == null)
        {
            Offers = new List<ContractExtensionOfferData>();
        }

        if (LastOfferAtUtc == null)
        {
            LastOfferAtUtc = "";
        }
    }
}
