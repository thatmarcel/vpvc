using ProtoBuf;

namespace VPVC.BackendCommunication.Shared.ProtobufMessages.ClientToServer; 

[ProtoContract]
public class PartyJoinMessageContent {
    [ProtoMember(1)]
    public string? partyJoinCode { get; set; }
    
    [ProtoMember(2)]
    public string? userDisplayName { get; set; }
}