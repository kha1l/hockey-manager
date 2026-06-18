using System;

[Serializable]
public class GmCareerEventData
{
    public string EventId = Guid.NewGuid().ToString("N");
    public string EventType;
    public int SeasonStartYear;
    public int SeasonEndYear;
    public string TeamId;
    public string TeamName;
    public string Title;
    public string Summary;
    public int TrustBefore;
    public int TrustAfter;
    public int JobSecurityBefore;
    public int JobSecurityAfter;
    public string CreatedAtUtc = DateTime.UtcNow.ToString("o");
}
