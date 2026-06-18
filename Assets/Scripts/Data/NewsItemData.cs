using System;

[Serializable]
public class NewsItemData
{
    public string NewsId = Guid.NewGuid().ToString("N");
    public int SeasonStartYear;
    public int SeasonEndYear;
    public string DateLabel;
    public string Category;
    public string Title;
    public string Body;
    public string TeamId;
    public string TeamName;
    public string PlayerId;
    public string PlayerName;
    public string RelatedId;
    public int Importance;
    public bool IsUserTeamRelated;
    public bool IsRead;
    public string CreatedAtUtc = DateTime.UtcNow.ToString("o");
}
