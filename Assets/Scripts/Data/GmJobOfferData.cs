using System;

[Serializable]
public class GmJobOfferData
{
    public string OfferId = Guid.NewGuid().ToString("N");
    public string TeamId;
    public string TeamName;
    public string TeamDirection;
    public int TeamOverall;
    public int LastSeasonPoints;
    public bool MadePlayoffsLastSeason;
    public int OwnerTrustStartingValue;
    public int JobSecurityStartingValue;
    public string OfferReason;
    public string ChallengeSummary;
    public string ExpectationsSummary;
    public bool IsAccepted;
    public bool IsDeclined;
    public string CreatedAtUtc = DateTime.UtcNow.ToString("o");
    public string ExpiresAtUtc;
}
