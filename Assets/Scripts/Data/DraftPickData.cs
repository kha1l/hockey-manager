using System;

[Serializable]
public class DraftPickData
{
    public string PickId;
    public int Round;
    public int PickInRound;
    public int OverallPick;
    public string OriginalTeamId;
    public string OriginalTeamName;
    public string TeamId;
    public string TeamName;
    public bool IsUserTeamPick;
    public bool IsCompleted;
    public string SelectedProspectId;
    public string SelectedProspectName;
}
