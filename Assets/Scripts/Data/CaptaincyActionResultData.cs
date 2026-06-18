using System;

[Serializable]
public class CaptaincyActionResultData
{
    public bool Success;
    public string Message;
    public string TeamId;
    public string TeamName;
    public string PlayerId;
    public string PlayerName;
    public string ActionType;
    public string AssignedRole;
    public string UpdatedAtUtc;

    public CaptaincyActionResultData()
    {
        UpdatedAtUtc = DateTime.UtcNow.ToString("o");
    }
}
