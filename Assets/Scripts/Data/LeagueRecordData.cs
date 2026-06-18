using System;

[Serializable]
public class LeagueRecordData
{
    public string RecordId = Guid.NewGuid().ToString("N");
    public string RecordType;
    public string RecordName;
    public string PlayerId;
    public string PlayerName;
    public string TeamId;
    public string TeamName;
    public int SeasonStartYear;
    public int SeasonEndYear;
    public int Value;
    public string ValueLabel;
    public bool IsCareerRecord;
    public string CreatedAtUtc = DateTime.UtcNow.ToString("o");
}
