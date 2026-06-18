using System;

[Serializable]
public class AwardWinnerData
{
    public string AwardId = Guid.NewGuid().ToString("N");
    public string AwardType;
    public string AwardName;
    public int SeasonStartYear;
    public int SeasonEndYear;
    public string PlayerId;
    public string PlayerName;
    public string TeamId;
    public string TeamName;
    public string Position;
    public int Age;
    public int Overall;
    public int Goals;
    public int Assists;
    public int Points;
    public int Wins;
    public int Shutouts;
    public int AwardScore;
    public string Reason;
    public string CreatedAtUtc = DateTime.UtcNow.ToString("o");
}
