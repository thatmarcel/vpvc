using ProtoBuf;

// Disable warning about fields being able to be null
// as we know they won't be because they are sent by the server
#pragma warning disable CS8618

namespace VPVC.BackendCommunication.Shared; 

[ProtoContract]
public class SerializablePartyParticipant {
    [ProtoMember(1)]
    public string id { get; set; }
    
    [ProtoMember(2)]
    public string userDisplayName { get; set; }
    
    [ProtoMember(3)]
    public int teamIndex { get; set; }
    
    [ProtoMember(4)]
    public bool isPartyLeader { get; set; }
}