using System;

[Serializable]
public class RetiredPlayerData
{
    public string PlayerId;
    public string PlayerName;
    public string Position;
    public int Age;
    public int JerseyNumber;
    public string PrimaryTeamId;
    public string PrimaryTeamName;
    public int SeasonsWithPrimaryTeam;
    public string LastTeamId;
    public string LastTeamName;
    public int RetirementSeasonStartYear;
    public int RetirementSeasonEndYear;
    public string RetirementReason;
    public int CareerGamesPlayed;
    public int CareerGoals;
    public int CareerAssists;
    public int CareerPoints;
    public int CareerWins;
    public int CareerShutouts;
    public int CareerAwardsCount;
    public int ChampionshipsWon;
    public int PlayoffRoundsWonCareer;
    public int HallOfFameScore;
    public bool IsHallOfFameEligible;
    public bool IsHallOfFameInducted;
    public int HallOfFameInductionYear;
    public bool HasRetiredNumber;
    public string RetiredNumberTeamId;
    public string RetiredNumberTeamName;
    public string CareerSummary;
    public string CreatedAtUtc = DateTime.UtcNow.ToString("o");
}
