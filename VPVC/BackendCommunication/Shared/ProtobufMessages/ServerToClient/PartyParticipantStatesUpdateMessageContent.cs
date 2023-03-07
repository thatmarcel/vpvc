using System.Collections.Generic;
using ProtoBuf;

namespace VPVC.BackendCommunication.Shared.ProtobufMessages.ServerToClient; 

[ProtoContract]
public class PartyParticipantStatesUpdateMessageContent {
    [ProtoMember(1)]
    public List<SerializablePartyParticipantState>? partyParticipantStates { get; set; }
}