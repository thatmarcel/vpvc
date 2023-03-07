using ProtoBuf;

namespace VPVC.BackendCommunication.Shared.ProtobufMessages.ServerToClient; 

[ProtoContract]
public class IncomingWebRtcSignalingMessageContent {
    [ProtoMember(1)]
    public string? sendingParticipantId { get; set; }
    
    [ProtoMember(2)]
    public string? signalingMessageType { get; set; }
    
    [ProtoMember(3)]
    public string? sdpContent { get; set; }
}