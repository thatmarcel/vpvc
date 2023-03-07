using System.Collections.Generic;
using ProtoBuf;

namespace VPVC.BackendCommunication.Shared.ProtobufMessages.ServerToClient; 

[ProtoContract]
public class PartyParticipantsChangeMessageContent {
    [ProtoMember(1)]
    public SerializablePartyParticipant? partyParticipantSelf { get; set; }
    
    [ProtoMember(2)]
    public List<SerializablePartyParticipant>? partyParticipants { get; set; }
}