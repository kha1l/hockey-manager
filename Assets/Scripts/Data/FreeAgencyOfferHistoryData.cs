using System;
using System.Collections.Generic;

[Serializable]
public class FreeAgencyOfferHistoryData
{
    public List<FreeAgentOfferData> Offers = new List<FreeAgentOfferData>();
    public int TotalOffers;
    public int AcceptedOffers;
    public int RejectedOffers;
    public string LastOfferAtUtc;

    public FreeAgencyOfferHistoryData()
    {
        EnsureOffers();
    }

    public void EnsureOffers()
    {
        if (Offers == null)
        {
            Offers = new List<FreeAgentOfferData>();
        }

        TotalOffers = Offers.Count;
        AcceptedOffers = 0;
        RejectedOffers = 0;
        LastOfferAtUtc = "";

        foreach (FreeAgentOfferData offer in Offers)
        {
            if (offer == null)
            {
                continue;
            }

            if (offer.Decision == "Accepted" || offer.Accepted)
            {
                AcceptedOffers++;
            }
            else if (offer.Decision == "Rejected")
            {
                RejectedOffers++;
            }

            if (!string.IsNullOrEmpty(offer.CreatedAtUtc))
            {
                LastOfferAtUtc = offer.CreatedAtUtc;
            }
        }
    }
}
