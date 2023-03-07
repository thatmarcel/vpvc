namespace VPVC.BackendCommunication.Shared; 

public static class MessageTypes {
    // Client-to-server
    public static readonly int partyCreate = 0;
    public static readonly int partyJoin = 1;
    public static readonly int selfStateUpdate = 2;
    public static readonly int outgoingWebRtcSignalingMessage = 3;
    public static readonly int changeTeam = 4;
    
    // Server-to-client
    public static readonly int partyCreateResult = 5;
    public static readonly int partyJoinResult = 6;
    public static readonly int incomingWebRtcSignalingMessage = 7;
    public static readonly int partyParticipantsChange = 8;
    public static readonly int partyParticipantStatesUpdate = 9;
    public static readonly int changeTeamResult = 10;
}