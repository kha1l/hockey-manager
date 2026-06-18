using System;

[Serializable]
public class HallOfFameInducteeData
{
    public string InducteeId = Guid.NewGuid().ToString("N");
    public string PlayerId;
    public string PlayerName;
    public string Position;
    public int JerseyNumber;
    public string PrimaryTeamId;
    public string PrimaryTeamName;
    public int InductionYear;
    public int RetirementSeasonStartYear;
    public int HallOfFameScore;
    public int CareerGamesPlayed;
    public int CareerGoals;
    public int CareerAssists;
    public int CareerPoints;
    public int CareerWins;
    public int CareerShutouts;
    public int CareerAwardsCount;
    public int ChampionshipsWon;
    public string InductionSummary;
    public string CreatedAtUtc = DateTime.UtcNow.ToString("o");
}
