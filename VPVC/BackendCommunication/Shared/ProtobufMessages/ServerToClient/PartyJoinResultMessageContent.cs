using System.Collections.Generic;
using ProtoBuf;

namespace VPVC.BackendCommunication.Shared.ProtobufMessages.ServerToClient; 

[ProtoContract]
public class PartyJoinResultMessageContent {
    [ProtoMember(1)]
    public bool success { get; set; }
    
    [ProtoMember(2)]
    public SerializablePartyParticipant? partyParticipantSelf { get; set; }
    
    [ProtoMember(3)]
    public List<SerializablePartyParticipant>? partyParticipants { get; set; }
}