using System;

[Serializable]
public class CpuRosterActionData
{
    public string ActionId = Guid.NewGuid().ToString("N");
    public string TeamId;
    public string TeamName;
    public string ActionType;
    public string PlayerId;
    public string PlayerName;
    public string FromStatus;
    public string ToStatus;
    public string Reason;
    public bool Success;
    public string Message;
    public string CreatedAtUtc = DateTime.UtcNow.ToString("o");

    public CpuRosterActionData()
    {
        if (string.IsNullOrEmpty(ActionId))
        {
            ActionId = Guid.NewGuid().ToString("N");
        }

        if (string.IsNullOrEmpty(CreatedAtUtc))
        {
            CreatedAtUtc = DateTime.UtcNow.ToString("o");
        }
    }
}
