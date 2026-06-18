using System;
using System.Collections.Generic;

[Serializable]
public class SeasonAwardsData
{
    public int SeasonStartYear;
    public int SeasonEndYear;
    public List<AwardWinnerData> Awards = new List<AwardWinnerData>();
    public string CreatedAtUtc = DateTime.UtcNow.ToString("o");

    public SeasonAwardsData()
    {
        EnsureAwards();
    }

    public void EnsureAwards()
    {
        if (Awards == null)
        {
            Awards = new List<AwardWinnerData>();
        }
    }
}
