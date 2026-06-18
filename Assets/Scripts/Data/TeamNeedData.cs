using System;

[Serializable]
public class TeamNeedData
{
    public string TeamId;
    public string TeamName;
    public string Direction;
    public int NeedTop6Forward;
    public int NeedBottom6Forward;
    public int NeedDefenseman;
    public int NeedGoalie;
    public int NeedProspects;
    public int NeedDraftPicks;
    public int NeedCapSpace;
    public int NeedRosterSpace;
    public int NeedYoungPlayers;
    public int NeedVeteranHelp;
    public int OverallNeedScore;
    public string PrimaryNeed;
    public string SecondaryNeed;
    public string UpdatedAtUtc = DateTime.UtcNow.ToString("o");

    public TeamNeedData()
    {
        if (string.IsNullOrEmpty(UpdatedAtUtc))
        {
            UpdatedAtUtc = DateTime.UtcNow.ToString("o");
        }
    }
}
