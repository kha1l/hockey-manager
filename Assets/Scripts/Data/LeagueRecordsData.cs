using System;
using System.Collections.Generic;

[Serializable]
public class LeagueRecordsData
{
    public List<LeagueRecordData> Records = new List<LeagueRecordData>();
    public string UpdatedAtUtc = DateTime.UtcNow.ToString("o");

    public LeagueRecordsData()
    {
        EnsureRecords();
    }

    public void EnsureRecords()
    {
        if (Records == null)
        {
            Records = new List<LeagueRecordData>();
        }
    }
}
