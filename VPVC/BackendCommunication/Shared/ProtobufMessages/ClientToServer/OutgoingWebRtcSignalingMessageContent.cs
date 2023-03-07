using ProtoBuf;

namespace VPVC.BackendCommunication.Shared.ProtobufMessages.ClientToServer; 

[ProtoContract]
public class OutgoingWebRtcSignalingMessageContent {
    [ProtoMember(1)]
    public string? receivingParticipantId { get; set; }
    
    [ProtoMember(2)]
    public string? signalingMessageType { get; set; }
    
    [ProtoMember(3)]
    public string? sdpContent { get; set; }
}