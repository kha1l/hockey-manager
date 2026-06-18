using System;

[Serializable]
public class RetiredNumberData
{
    public string RetiredNumberId = Guid.NewGuid().ToString("N");
    public string TeamId;
    public string TeamName;
    public string PlayerId;
    public string PlayerName;
    public int JerseyNumber;
    public int RetirementSeasonStartYear;
    public int RetirementSeasonEndYear;
    public int RetiredNumberYear;
    public int RetiredNumberScore;
    public string Reason;
    public string CreatedAtUtc = DateTime.UtcNow.ToString("o");
}
