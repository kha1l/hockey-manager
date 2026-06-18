using System;

[Serializable]
public class RosterMoveResultData
{
    public bool Success;
    public string Message;
    public string PlayerId;
    public string PlayerName;
    public string FromStatus;
    public string ToStatus;
    public string UpdatedAtUtc;
}
