using System;

[Serializable]
public class DraftPickOwnershipData
{
    public string PickId;
    public int DraftYear;
    public int Round;
    public string OriginalTeamId;
    public string OriginalTeamName;
    public string CurrentOwnerTeamId;
    public string CurrentOwnerTeamName;
    public bool IsTraded;
    public bool IsUsed;
    public string LastTradeId;
}
