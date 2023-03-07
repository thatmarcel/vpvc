using ProtoBuf;

// Disable warning about fields being able to be null
// as we know they won't be because they are sent by the server
#pragma warning disable CS8618

namespace VPVC.BackendCommunication.Shared; 

[ProtoContract]
public class SerializablePartyParticipantState {
    [ProtoMember(1)]
    public string participantId { get; set; }
    
    [ProtoMember(2)]
    public int gameState { get; set; }
    
    [ProtoMember(3)]
    public int relativePositionX { get; set; }
    
    [ProtoMember(4)]
    public int relativePositionY { get; set; }
}