using System;

[Serializable]
public class InjuryRecordData
{
    public string InjuryId;
    public string PlayerId;
    public string PlayerName;
    public string TeamId;
    public string TeamName;
    public string Position;
    public int Age;
    public string InjuryType;
    public string InjurySeverity;
    public int InjuryDays;
    public int InjuryDaysRemainingAtCreation;
    public string InjuredAtUtc;
    public string ExpectedReturnDate;
    public string Status;
    public string Source;
    public string OriginalRosterStatus;
    public string OriginalSlotType;
    public int OriginalLineOrPairNumber;
    public string OriginalSlotPosition;
    public string ReplacementPlayerId;
    public string ReplacementPlayerName;
}
