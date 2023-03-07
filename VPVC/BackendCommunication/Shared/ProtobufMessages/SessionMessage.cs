using ProtoBuf;
using VPVC.BackendCommunication.Shared.ProtobufMessages.ClientToServer;
using VPVC.BackendCommunication.Shared.ProtobufMessages.ServerToClient;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnassignedField.Global

namespace VPVC.BackendCommunication.Shared.ProtobufMessages; 

[ProtoContract]
public class SessionMessage {
    [ProtoMember(2)]
    public int type { get; set; }

    [ProtoMember(3)]
    public PartyCreateMessageContent? partyCreateMessageContent { get; set; }
    
    [ProtoMember(4)]
    public PartyCreateResultMessageContent? partyCreateResultMessageContent { get; set; }
    
    [ProtoMember(5)]
    public PartyJoinMessageContent? partyJoinMessageContent { get; set; }
    
    [ProtoMember(6)]
    public PartyJoinResultMessageContent? partyJoinResultMessageContent { get; set; }
    
    [ProtoMember(7)]
    public SelfStateUpdateMessageContent? selfStateUpdateMessageContent { get; set; }
    
    [ProtoMember(8)]
    public OutgoingWebRtcSignalingMessageContent? outgoingWebRtcSignalingMessageContent { get; set; }
    
    [ProtoMember(9)]
    public IncomingWebRtcSignalingMessageContent? incomingWebRtcSignalingMessageContent { get; set; }
    
    [ProtoMember(10)]
    public PartyParticipantsChangeMessageContent? partyParticipantsChangeMessageContent { get; set; }
    
    [ProtoMember(11)]
    public PartyParticipantStatesUpdateMessageContent? partyParticipantStatesUpdateMessageContent { get; set; }
    
    [ProtoMember(12)]
    public ChangeTeamMessageContent? changeTeamMessageContent { get; set; }
    
    [ProtoMember(13)]
    public ChangeTeamResultMessageContent? changeTeamResultMessageContent { get; set; }
}