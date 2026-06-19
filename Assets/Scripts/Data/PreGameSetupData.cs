using System;

[Serializable]
public class PreGameSetupData
{
    public string ScheduledGameId;
    public bool IsAvailable;
    public string AvailabilityMessage;
    public bool IsPlayoffGame;
    public string HomeTeamId;
    public string HomeTeamName;
    public string HomeLogoResourcePath;
    public string HomeJerseyResourcePath;
    public string HomeFullBodyResourcePath;
    public string HomePreviewStatsText;
    public string AwayTeamId;
    public string AwayTeamName;
    public string AwayLogoResourcePath;
    public string AwayJerseyResourcePath;
    public string AwayFullBodyResourcePath;
    public string AwayPreviewStatsText;
    public string UserTeamId;
    public string UserTeamName;
    public bool IsUserHomeTeam;
    public string OpponentTeamId;
    public string OpponentTeamName;
    public string CurrentTacticName;
    public string StartingGoaliePlayerId;
    public string StartingGoalieName;
    public string BackupGoaliePlayerId;
    public string BackupGoalieName;
    public bool IsLineupValid;
    public string LineupValidationMessage;
    public bool CanStartMatch;
    public string Summary;
}
