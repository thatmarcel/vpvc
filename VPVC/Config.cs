namespace VPVC; 

public static class Config {
    public static readonly int teamCount = 2;
    public static readonly int maxParticipantsPerTeam = 5;
    public static readonly int maxParticipantsPerParty = maxParticipantsPerTeam * teamCount;
    
    public static readonly int minPartyJoinCodeLength = 4;
    public static readonly int maxPartyJoinCodeLength = 12;
    public static readonly int minUserDisplayNameLength = 3;
    public static readonly int maxUserDisplayNameLength = 16;

    public static readonly int gameCoordinateExtractionIntervalInMilliseconds = 500;

    public static readonly string serverHostname = "backend.vpvc.app";
    public static readonly int serverPort = 443;
    
    public static readonly int fullVolumeHearingRadius = 6;
    public static readonly int maxHearingRadius = 20;
}