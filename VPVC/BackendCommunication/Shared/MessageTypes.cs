namespace VPVC.BackendCommunication.Shared; 

public static class MessageTypes {
    // Client-to-server
    public static readonly int partyCreate = 0;
    public static readonly int partyJoin = 1;
    public static readonly int selfStateUpdate = 2;
    public static readonly int changeTeam = 3;
    
    // Server-to-client
    public static readonly int partyCreateResult = 4;
    public static readonly int partyJoinResult = 5;
    public static readonly int partyParticipantsChange = 6;
    public static readonly int partyParticipantStatesUpdate = 7;
    public static readonly int changeTeamResult = 8;
}