using System;

[Serializable]
public class TeamUsageSummaryData
{
    public string TeamId;
    public string TeamName;
    public int AverageActiveTimeOnIceSeconds;
    public int TopForwardTimeOnIceSeconds;
    public int TopDefenseTimeOnIceSeconds;
    public int StartingGoalieTimeOnIceSeconds;
    public int ActivePlayerCount;
    public int ScratchPlayerCount;
    public string UpdatedAtUtc = DateTime.UtcNow.ToString("o");
}
