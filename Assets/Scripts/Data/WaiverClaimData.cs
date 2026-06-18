using System;

[Serializable]
public class WaiverClaimData
{
    public string ClaimId;
    public string WaiverId;
    public string PlayerId;
    public string PlayerName;
    public string ClaimingTeamId;
    public string ClaimingTeamName;
    public string OriginalTeamId;
    public string OriginalTeamName;
    public string CreatedAtUtc;
    public string Status;
    public int ClaimScore;
}
