namespace VPVC; 

public static class Config {
    // public static readonly int teamCount = 2;
    public static readonly int maxParticipantsPerTeam = 5;
    // public static readonly int maxParticipantsPerParty = maxParticipantsPerTeam * teamCount;
    
    public static readonly int minPartyJoinCodeLength = 4;
    public static readonly int maxPartyJoinCodeLength = 12;
    public static readonly int minUserDisplayNameLength = 3;
    public static readonly int maxUserDisplayNameLength = 16;

    public static readonly int gameCoordinateExtractionIntervalInMilliseconds = 200;
    
    public static readonly int backendServerPort = 443;
    public static readonly int voiceChatBackendServerPort = 4719;
    
    public static readonly int fullVolumeHearingRadius = 2;
    public static readonly int maxHearingRadius = 16;
}